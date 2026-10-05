using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using NLog;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The "mods update info editor" symbol: opens the window in which the update
    /// coordinates the updater works from can be read and edited, mod by mod.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The window is modeless, so one instance of this class holds the state of one
    /// editor session: the list of mod blocks, the filter and the controls it built.
    /// A second session is refused rather than started, because two of them would
    /// share those fields and the first window to close would take the second one's
    /// controls down with it.
    /// </para>
    /// <para>
    /// The editor knows no update host. Each mod block gets one tab per source which holds
    /// data for that mod, and the control inside a tab is built by that source; the add tab
    /// is the same idea seen from the other side, one tab per source holding the form which
    /// adds a mod of its kind. What stays here is the part which is the same for all of
    /// them: the window, the filter, the log box, the mods folder, and the storing of an
    /// edited value.
    /// </para>
    /// <para>
    /// Everything which is not this window lives in its own file: the source abstraction in
    /// <c>IUpdateSource.cs</c> and <c>UpdateSourceData.cs</c>, the controls a source builds in
    /// <c>UpdateSourceInfoControl.cs</c> and <c>UpdateSourceAddControl.cs</c>, the github
    /// implementation in <c>GithubUpdateSource.cs</c>, <c>GithubUpdateData.cs</c> and
    /// <c>GithubAddModControl.cs</c>, one mod's block in <c>ModInfoBlock.cs</c>, its buttons
    /// in <c>ModUpdateInfoButtons.cs</c>, the measurements in <c>EditorLayout.cs</c>, and the
    /// add tab with the adding of a mod in <c>InfoEditorForFlp.AddModTab.cs</c>.
    /// </para>
    /// </remarks>
    internal partial class InfoEditorForFlp : FunctionForFlpBase, IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Height of the header row which holds the filter field.
        /// </summary>
        const int HeaderRowHeight = 30;

        /// <summary>
        /// Width of the filter field in the header row.
        /// </summary>
        const int FilterBoxWidth = 320;

        /// <summary>
        /// How long the editor waits after the last keystroke before it stores a value.
        /// </summary>
        /// <remarks>
        /// A text field raises its change event once per character, and every one of them
        /// would otherwise rewrite meta.ini. The wait is what turns a burst of keystrokes
        /// into one write, and the value written at the end of it is the last one, because
        /// the field pushes what was typed into the data as it is typed.
        /// </remarks>
        static readonly TimeSpan WriteBackDelay = TimeSpan.FromSeconds(1);

        public override string Symbol => T._("⚒");

        public override string Description => T._("Open mods update info editor");

        public override Color? ForeColor => Color.LightSkyBlue;

        public override void OnClick(object o, EventArgs e)
        {
            OpenInfoEditor();
        }

        // Cache of already built mod info blocks. They are built once and reused; for the
        // mods added from the 'add mod' tab a new block is appended once the mod was
        // downloaded and added, instead of reading all infos again.
        readonly List<ModInfoBlock> _modInfoBlocks = new List<ModInfoBlock>();

        bool _isReading;
        bool _isWriting;

        // Data whose fields were edited while a write was already waiting. A write covers one
        // source's data, so without this a second mod edited in the second after another one
        // would keep its old value in the ini.
        readonly HashSet<UpdateSourceData> _pendingWrites = new HashSet<UpdateSourceData>();

        // Controls of the currently opened editor (used by the filter and by the 'add mod'
        // tab). They are children of the editor form and are released when a session ends.
        FlowLayoutPanel _modsListFlowPanel;
        TextBox _filterTextBox;

        // The tooltip is a component rather than a control, so the editor form does not
        // dispose it and this object has to.
        ToolTip _toolTip;

        // The log box of the current session, hosted by the add-mod tab.
        RichTextBox _logBox;

        /// <summary>
        /// Releases what an editor session created that the editor form cannot release itself.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para>
        /// Releases what an editor session created and lets go of its controls.
        /// </para>
        /// <para>
        /// The tooltip is the one thing the editor form cannot release itself: a
        /// <see cref="ToolTip"/> is a component and not a child control, so it stays alive
        /// after the form closes. The panel, the filter box and the log box are children of
        /// that form and are disposed with it; disposing them again here is harmless, because
        /// <see cref="Control.Dispose()"/> is idempotent, and it is what makes the ownership
        /// this object has over them explicit. The cached mod info blocks stay on
        /// <see cref="_modInfoBlocks"/> until the next session clears them.
        /// </para>
        /// </summary>
        /// <param name="disposing">
        /// false when called from a finalizer, where only unmanaged resources may be touched.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _toolTip?.Dispose();
            _toolTip = null;

            _filterTextBox?.Dispose();
            _filterTextBox = null;

            _modsListFlowPanel?.Dispose();
            _modsListFlowPanel = null;

            _logBox?.Dispose();
            _logBox = null;
        }

        /// <summary>
        /// Opens the editor window, or brings the one already open to the front.
        /// </summary>
        internal void OpenInfoEditor()
        {
            // The editor is modeless, so the symbol can be pressed again while a session is
            // open. Two sessions would share the fields of this instance, and the first
            // window to close would dispose the second one's controls, so the window which is
            // already open is brought forward instead of a second one being built. The list
            // panel is what says whether there is one: it is created with the session and
            // released with it, and it walks up to its own window.
            var openEditorForm = _modsListFlowPanel == null ? null : _modsListFlowPanel.FindForm();
            if (openEditorForm != null)
            {
                openEditorForm.Activate();
                return;
            }

            // A fresh editor session: reset caches so mod infos are read once per form.
            _modInfoBlocks.Clear();
            _pendingWrites.Clear();

            var modUpdateInfosForm = CreateEditorForm();

            // Build and cache all mod infos once.
            LoadModInfos(modUpdateInfosForm);

            // applied after the infos are built, so the blocks it has to reach exist
            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, modUpdateInfosForm);

            modUpdateInfosForm.Size = new Size(modUpdateInfosForm.Width * 2, modUpdateInfosForm.Height * 2);

            // The editor is shown modeless, so its session ends with the form rather than
            // with this method. The tooltip is not a child of that form and has to be
            // released explicitly.
            modUpdateInfosForm.FormClosed += (o, e) => Dispose();

            modUpdateInfosForm.Show(ManageSettings.MainForm);
        }

        /// <summary>
        /// Builds the editor window: a filter row above a tab strip which holds the mod
        /// list and the add-mod forms.
        /// </summary>
        /// <remarks>
        /// The strip is a <see cref="ThemedTabControl"/> rather than a plain
        /// <see cref="TabControl"/>. A plain strip paints the area behind its headers
        /// itself and ignores the colour it is given there, so that area kept the system
        /// colour whatever theme was applied, while the headers sitting on it were themed.
        /// </remarks>
        private Form CreateEditorForm()
        {
            var modUpdateInfosForm = new Form
            {
                Text = T._("Mods update info settings") + " (" + ManageSettings.CurrentGameDisplayingName + ")",
                StartPosition = FormStartPosition.CenterScreen
            };

            var modUpdateInfosTableLayoutPanel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            modUpdateInfosTableLayoutPanel.RowStyles.Add(new RowStyle
            {
                SizeType = SizeType.Absolute,
                Height = HeaderRowHeight,
            });
            modUpdateInfosTableLayoutPanel.RowStyles.Add(new RowStyle
            {
                SizeType = SizeType.Percent,
                Height = 100,
            });

            _modsListFlowPanel = CreateModsListPanel();

            modUpdateInfosTableLayoutPanel.Controls.Add(CreateFilterRow(), 0, 0);
            modUpdateInfosTableLayoutPanel.Controls.Add(CreateContentTabs(), 0, 1);
            modUpdateInfosForm.Controls.Add(modUpdateInfosTableLayoutPanel);

            return modUpdateInfosForm;
        }

        /// <summary>
        /// The scrolling panel every mod info block is added to.
        /// </summary>
        private static FlowLayoutPanel CreateModsListPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Margin = new Padding(0),
            };
        }

        /// <summary>
        /// The header row: the filter field which dynamically filters the displayed mod infos.
        /// </summary>
        /// <remarks>
        /// The row lays its children out in flow order, so neither of them carries an
        /// anchor: a flow layout ignores one and positions the control itself.
        /// </remarks>
        private FlowLayoutPanel CreateFilterRow()
        {
            var filterFlowLayoutPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };

            var filterLabel = new Label
            {
                Text = T._("Filter") + ":",
                AutoSize = true,
            };

            _filterTextBox = new TextBox
            {
                Width = FilterBoxWidth,
            };
            _filterTextBox.TextChanged += (o, e) => ApplyFilter();

            filterFlowLayoutPanel.Controls.Add(filterLabel);
            filterFlowLayoutPanel.Controls.Add(_filterTextBox);

            return filterFlowLayoutPanel;
        }

        /// <summary>
        /// The content tab strip: the mod list, and the add-mod tab built on the first
        /// visit to it.
        /// </summary>
        /// <remarks>
        /// The tab pages carry no visual style setting of their own. The theme turns that
        /// style off for every page it reaches, because a page which keeps it ignores the
        /// background colour the theme gives it.
        /// </remarks>
        private ThemedTabControl CreateContentTabs()
        {
            var modUpdateInfosTabControl = new ThemedTabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };

            var allModsTabPage = new TabPage { Text = T._("All") };
            var addModTabPage = new TabPage { Text = T._("Add") };

            allModsTabPage.Controls.Add(_modsListFlowPanel);

            modUpdateInfosTabControl.TabPages.Add(allModsTabPage);
            modUpdateInfosTabControl.TabPages.Add(addModTabPage);

            // Build "Add mod" tab page lazily on its first selection: every source is asked
            // for its form there, and a source which cannot be reached would otherwise delay
            // a window the user opened to look at the list.
            modUpdateInfosTabControl.SelectedIndexChanged += (o, e) =>
            {
                if (modUpdateInfosTabControl.SelectedTab == addModTabPage && addModTabPage.Controls.Count == 0)
                {
                    LoadAddModTab(addModTabPage);
                }
            };

            return modUpdateInfosTabControl;
        }

        private static ToolTip CreateToolTip()
        {
            return new ToolTip
            {
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                ShowAlways = true
            };
        }

        /// <summary>
        /// Makes sure the log box exists, creating it when the previous editor session
        /// took it down together with its form.
        /// </summary>
        /// <remarks>
        /// The test is for a missing or disposed box rather than for an accessible one. The
        /// box is a child of the add-mod tab, so it is not accessible while that tab is not
        /// the selected one, and treating that as "gone" would throw away the log every time
        /// the user looked at another tab.
        /// </remarks>
        private RichTextBox EnsureLogBox()
        {
            if (_logBox != null && !_logBox.IsDisposed) return _logBox;

            _logBox = new RichTextBox { ReadOnly = true, Width = 200 };

            return _logBox;
        }

        /// <summary>
        /// Appends a line to the log box, creating the box when it is not there yet.
        /// </summary>
        private void Log(string line)
        {
            // AppendText rather than Text +=: the latter rebuilds the whole content on every
            // line and moves the caret back to the start, so the box would both get slower
            // as it fills and jump away from what the user is reading.
            EnsureLogBox().AppendText(line + Environment.NewLine);
        }

        private void ApplyFilter()
        {
            if (_modsListFlowPanel == null) return;

            var filterText = _filterTextBox == null ? "" : _filterTextBox.Text.Trim();

            _modsListFlowPanel.SuspendLayout();
            foreach (var modInfoBlock in _modInfoBlocks)
            {
                modInfoBlock.Control.Visible =
                    filterText.Length == 0
                    || modInfoBlock.Filter.IndexOf(filterText, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            _modsListFlowPanel.ResumeLayout();
        }

        /// <summary>
        /// Builds one mod info block with the write back muted for the length of the build.
        /// </summary>
        /// <remarks>
        /// Building a block wires its fields to the data, and attaching a binding pushes the
        /// stored value into the field, which raises the change event and would write that
        /// value straight back to the ini. Muting the write back is what keeps reading a
        /// record from looking like the user editing it. The mute lives here, around the one
        /// call which needs it, so no caller can leave the flag set.
        /// </remarks>
        private ModInfoBlock CreateModInfoBlock(UpdateInfoData infoData)
        {
            _isReading = true;
            try
            {
                return new ModInfoBlock(infoData, _toolTip, WriteValue);
            }
            finally
            {
                _isReading = false;
            }
        }

        /// <summary>
        /// Adds the block of a mod that was just downloaded and added to the open editor.
        /// </summary>
        private void AddNewModInfoBlock(UpdateSourceData data)
        {
            if (_modsListFlowPanel == null) return;

            if (_toolTip == null) _toolTip = CreateToolTip();

            var infoData = new UpdateInfoData(data.Mod);
            infoData.Sources.Add(data);

            var modInfoBlock = CreateModInfoBlock(infoData);

            _modInfoBlocks.Add(modInfoBlock);
            _modsListFlowPanel.Controls.Add(modInfoBlock.Control);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, modInfoBlock.Control);

            ApplyFilter();
        }

        /// <summary>
        /// Reads what every update source holds for every mod and builds the mod info blocks.
        /// </summary>
        /// <remarks>
        /// The blocks are added to <see cref="_modsListFlowPanel"/>, which the editor form
        /// owns, rather than to a panel handed in: it is the one the filter works on and the
        /// one the added mods are appended to later. The form is only needed to widen it to
        /// the widest block, and theming it is left to the caller, which does it once the
        /// blocks exist.
        /// </remarks>
        private void LoadModInfos(Form form)
        {
            // Already read blocks are cached: do not rebuild them.
            if (_modInfoBlocks.Count > 0)
            {
                ApplyFilter();
                return;
            }

            _toolTip = CreateToolTip();

            var mods = new ModlistData();

            foreach (var mod in mods
                .Mods
                .Where(m =>
                !(m.Type is ModType.Separator)
                && !(m.Type is ModType.Overwrite)
                ))
            {
                var infoData = TryReadUpdateInfo(mod);
                if (infoData == null) continue;

                var modInfoBlock = CreateModInfoBlock(infoData);

                _modInfoBlocks.Add(modInfoBlock);
                _modsListFlowPanel.Controls.Add(modInfoBlock.Control);

                var blockWidth = modInfoBlock.Control.Width + modInfoBlock.Control.Margin.Horizontal * 2;
                var blockHeight = modInfoBlock.Control.Height + modInfoBlock.Control.Margin.Vertical * 2;
                form.Size = new Size
                    (
                    blockWidth > form.Width ? blockWidth : form.Width
                    , blockHeight > form.Height ? blockHeight : form.Height
                    );
            }
        }

        /// <summary>
        /// Collects the data every update source holds for one mod.
        /// </summary>
        /// <remarks>
        /// The sources are asked rather than searched: which files a host keeps its
        /// coordinates in, and what counts as a usable value there, is the host's own
        /// question. A mod no source has anything for is not one the editor can show.
        /// </remarks>
        /// <param name="mod">Mod to read.</param>
        /// <returns>The data, or null when no source has any for this mod.</returns>
        private static UpdateInfoData TryReadUpdateInfo(ModData mod)
        {
            var infoData = new UpdateInfoData(mod);

            foreach (var source in UpdateSourceCatalog.All)
            {
                UpdateSourceData sourceData;
                if (!source.TryReadData(mod, out sourceData)) continue;

                infoData.Sources.Add(sourceData);
            }

            return infoData.Sources.Count == 0 ? null : infoData;
        }

        /// <summary>
        /// Stores the data the user just edited, once the edits have settled.
        /// </summary>
        /// <param name="data">
        /// Data of the source whose field was edited. The value is already in it: the control
        /// pushes what is typed as it is typed, so the last edit of a burst is the one which
        /// is stored.
        /// </param>
        private async void WriteValue(UpdateSourceData data)
        {
            // The mute is what keeps the values pushed in while a block is being built from
            // being written back as if the user had edited them.
            if (_isReading) return;

            // The edit is remembered even when a write is already waiting, so that a second
            // mod edited during the wait is stored as well.
            _pendingWrites.Add(data);

            if (_isWriting) return;

            _isWriting = true;
            try
            {
                // ConfigureAwait(true) keeps the continuation on the UI thread, which the
                // data is only ever touched from; the flag is reset in the finally, so a
                // failure cannot leave the editor muted for the rest of the session.
                await Task.Delay(WriteBackDelay).ConfigureAwait(true);

                // Taken before the writes run: an edit made while they run belongs to the
                // next wait rather than to this one, which is about to be forgotten.
                var pending = new List<UpdateSourceData>(_pendingWrites);
                _pendingWrites.Clear();

                foreach (var pendingData in pending)
                {
                    pendingData.Write();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Failed to store the mod update info. Error:\r\n" + ex);
            }
            finally
            {
                _isWriting = false;
            }
        }
    }
}
