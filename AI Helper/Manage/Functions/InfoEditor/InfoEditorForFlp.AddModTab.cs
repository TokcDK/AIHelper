using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using INIFileMan;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The "Add" tab of the mod info editor: one tab per update source, each holding the
    /// form that source builds for adding a mod of its own kind, and the log box they all
    /// report into.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kept beside <see cref="InfoEditorForFlp"/> as a part of it rather than as a class of
    /// its own, because what happens here is the editor's half of adding a mod: the mods
    /// directory, the names already taken, and the meta.ini which is written once the files
    /// are in place. What a source owns is the other half — the form, and the download.
    /// </para>
    /// <para>
    /// The two halves meet at <see cref="UpdateSourceAddControl.AddRequested"/>: a source
    /// reads its form and says what the user asked for, and the editor decides whether it
    /// may be done and then does it.
    /// </para>
    /// </remarks>
    internal partial class InfoEditorForFlp
    {
        /// <summary>Height of the log box below the source tabs.</summary>
        const int LogBoxHeight = 120;

        /// <summary>
        /// Builds the add-mod tab: the strip of sources, and the log box under it.
        /// </summary>
        /// <remarks>
        /// The log box is shared by every source rather than repeated in each tab: what it
        /// reports is what happened to the mod being added, and which source it came from is
        /// already visible in the strip above it.
        /// </remarks>
        private void LoadAddModTab(Control addModContainer)
        {
            addModContainer.Controls.Clear();

            var addModLayout = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };
            addModLayout.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 100 });
            addModLayout.RowStyles.Add(new RowStyle { SizeType = SizeType.Absolute, Height = LogBoxHeight });

            var sourceTabs = new ThemedTabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };

            foreach (var source in UpdateSourceCatalog.All)
            {
                var addControl = source.CreateAddControl(Log);
                addControl.AddRequested += OnAddModRequested;

                var sourceTabPage = new TabPage
                {
                    Text = source.Name,
                    // The page scrolls rather than the form: a form sized to its content and
                    // docked to the top is what keeps the buttons off a band of empty space.
                    AutoScroll = true,
                };
                sourceTabPage.Controls.Add(addControl.Control);

                sourceTabs.TabPages.Add(sourceTabPage);
            }

            var logBox = EnsureLogBox();
            logBox.Dock = DockStyle.Fill;

            addModLayout.Controls.Add(sourceTabs, 0, 0);
            addModLayout.Controls.Add(logBox, 0, 1);

            addModContainer.Controls.Add(addModLayout);

            ThemesLoader.SetTheme(ManageSettings.CurrentTheme, addModContainer);
        }

        /// <summary>
        /// Handles a source asking for a mod to be added.
        /// </summary>
        /// <remarks>
        /// A form's event handler may not return a task, so the work is awaited here and the
        /// method which does it keeps its exceptions to itself: an exception escaping an
        /// async void handler would take the application down instead of being logged.
        /// </remarks>
        private async void OnAddModRequested(object sender, AddModRequestEventArgs e)
        {
            await AddModAsync(e).ConfigureAwait(true);
        }

        /// <summary>
        /// Adds the mod a source asked for: checks the name against the mods folder, asks the
        /// source to download and install it, and only then writes the record into meta.ini.
        /// </summary>
        /// <remarks>
        /// The order is the point. Resolving and installing first means a failed download
        /// never leaves a mod which claims to be updatable, and the record is what the update
        /// pass reads.
        /// </remarks>
        private async Task AddModAsync(AddModRequestEventArgs request)
        {
            try
            {
                var data = request.Data;

                // The name is typed by the user and is combined with the mods directory, so
                // whether it may be one is the editor's check and not the form's.
                if (!IsValidModDirName(request.ModDirectoryName))
                {
                    Log(T._("Mod dir name is not a valid folder name"));
                    return;
                }

                var targetDirPathInfo = new DirectoryInfo(
                    Path.Combine(ManageSettings.CurrentGameModsDirPath, request.ModDirectoryName));

                if (targetDirPathInfo.Exists)
                {
                    Log(T._("Target mod dir is exists!"));
                    return;
                }

                if (IsAlreadyListed(data)) return;

                data.Mod.Path = targetDirPathInfo.FullName;

                if (!await data.Source
                        .DownloadAndInstallAsync(data, targetDirPathInfo.FullName, Log, CancellationToken.None)
                        .ConfigureAwait(true))
                {
                    return;
                }

                data.Mod.MetaIni = new INIFile(Path.Combine(targetDirPathInfo.FullName, "meta.ini"), true);

                data.Write();

                Log(T._("Mod succesfully downloaded and added!"));

                // The mod was successfully downloaded and added: build one new block with the
                // entered values and append it to the cache.
                AddNewModInfoBlock(data);
            }
            catch (Exception ex)
            {
                Log(T._("Error: ") + ex.Message);
                _log.Error("Failed to download and add the mod. Error:\r\n" + ex);
            }
        }

        /// <summary>
        /// True when a mod the editor already shows is fed by the same release file, which
        /// makes the one being added a duplicate.
        /// </summary>
        private bool IsAlreadyListed(UpdateSourceData data)
        {
            foreach (var modInfoBlock in _modInfoBlocks)
            {
                foreach (var existingData in modInfoBlock.InfoData.Sources)
                {
                    if (!data.PointsAtSameFileAs(existingData)) continue;

                    Log(T._("Mod '{0}' has the same Owner, Repository, start/end", modInfoBlock.InfoData.ModName));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when the name can be used as the folder of a mod inside the mods directory.
        /// </summary>
        /// <remarks>
        /// The name is typed by the user and is combined with the mods directory, and
        /// <see cref="Path.Combine(string, string)"/> returns the second path unchanged when
        /// it is rooted, so a name carrying a drive or a separator would put the mod
        /// somewhere else entirely.
        /// </remarks>
        private static bool IsValidModDirName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name != name.Trim()) return false;
            if (name == "." || name == "..") return false;
            if (Path.IsPathRooted(name)) return false;
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;

            return true;
        }
    }
}
