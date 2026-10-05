using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The UI block of one mod: its name, one tab per update source which has data for it,
    /// and the buttons which open it.
    /// </summary>
    /// <remarks>
    /// A block is built once per editor session and then reused, so the editor is not
    /// rebuilt every time the filter runs. <see cref="Filter"/> is the text the block is
    /// matched against.
    /// </remarks>
    internal sealed class ModInfoBlock
    {
        /// <summary>Width of the caption of the block, which holds the mod name.</summary>
        private const int BlockLabelWidth = 100;

        /// <summary>Room the data panel keeps around the controls it holds.</summary>
        private const int DataPanelExtraWidth = 10;

        /// <inheritdoc cref="DataPanelExtraWidth"/>
        private const int DataPanelExtraHeight = 15;

        /// <summary>Room the block keeps around the mod name and the data panel.</summary>
        private const int BlockExtraSize = 15;

        /// <summary>Height the block is pinned to once its content has been measured.</summary>
        private const int BlockHeight = 300;

        /// <summary>
        /// Room the tab strip of the sources keeps around the page it shows: the headers
        /// above it and the border around it. A page is not laid out until its tab is
        /// selected, so the strip cannot be measured from the pages and is sized from what
        /// they will need instead.
        /// </summary>
        private const int SourceTabWidthReserve = 8;

        /// <inheritdoc cref="SourceTabWidthReserve"/>
        private const int SourceTabHeightReserve = 24;

        /// <summary>
        /// The buttons every block carries, in the order they are shown. They hold no
        /// state of their own, so one set is shared by all blocks.
        /// </summary>
        private static readonly IModUpdateInfoButton[] Buttons =
        {
            new OpenWebPage(),
            new OpenModDir(),
        };

        private readonly UpdateInfoData _infoData;
        private readonly ToolTip _toolTip;
        private readonly Action<UpdateSourceData> _dataChanged;

        private ThemedTabControl _sourceTabs;

        /// <param name="infoData">The mod and the data every source holds for it.</param>
        /// <param name="toolTip">Tooltip the block explains itself with.</param>
        /// <param name="dataChanged">
        /// Called with the data of the source whose field the user edited, so that the editor
        /// can store it once the edits have settled.
        /// </param>
        internal ModInfoBlock(UpdateInfoData infoData, ToolTip toolTip, Action<UpdateSourceData> dataChanged)
        {
            _infoData = infoData;
            _toolTip = toolTip;
            _dataChanged = dataChanged;

            Control = Build();
        }

        /// <summary>The control element with all added mod info elements.</summary>
        internal Control Control { get; }

        internal UpdateInfoData InfoData
        {
            get { return _infoData; }
        }

        /// <summary>
        /// Getter-only text used to filter the mod: the mod's directory name, the name of
        /// every source it has data of, and the text of that data.
        /// </summary>
        internal string Filter
        {
            get
            {
                var parts = new List<string> { _infoData.ModName };

                foreach (var sourceData in _infoData.Sources)
                {
                    parts.Add(sourceData.Source.Name);
                    parts.Add(sourceData.FilterText);
                }

                return string.Join(" ", parts);
            }
        }

        /// <summary>
        /// Data of the source whose tab is selected, which is what the block's buttons act
        /// on. Null while the block has no tab.
        /// </summary>
        private UpdateSourceData SelectedSource
        {
            get
            {
                var selectedPage = _sourceTabs == null ? null : _sourceTabs.SelectedTab;

                return selectedPage == null ? null : selectedPage.Tag as UpdateSourceData;
            }
        }

        private FlowLayoutPanel Build()
        {
            var blockPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                Size = new Size(EditorLayout.PanelSeedSize, EditorLayout.PanelSeedSize),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.Fixed3D,
            };

            var nameLabel = new Label
            {
                AutoSize = true,
                Text = $"{T._("Name")}: {_infoData.ModName}",
                Size = new Size(BlockLabelWidth, EditorLayout.ElementHeight * 3),
                Margin = new Padding(0),
            };
            _toolTip.SetToolTip(nameLabel, T._("Mod dir name"));
            blockPanel.Controls.Add(nameLabel);

            EditorLayout.GrowToCoverBoth(blockPanel, EditorLayout.OccupiedSize(nameLabel));

            var dataPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Size = new Size(EditorLayout.PanelSeedSize, EditorLayout.PanelSeedSize),
                Margin = new Padding(0),
                BorderStyle = BorderStyle.Fixed3D,
            };

            _sourceTabs = BuildSourceTabs();

            dataPanel.Controls.Add(_sourceTabs);
            EditorLayout.GrowToCover(dataPanel, EditorLayout.OccupiedSize(_sourceTabs));

            AddButtons(dataPanel);

            dataPanel.Size = new Size(
                dataPanel.Width + DataPanelExtraWidth,
                dataPanel.Height + DataPanelExtraHeight);

            blockPanel.Size = new Size(
                blockPanel.Width + dataPanel.Width + BlockExtraSize,
                blockPanel.Height + dataPanel.Height + BlockExtraSize);
            blockPanel.Size = new Size(blockPanel.Width + BlockExtraSize, BlockHeight);

            blockPanel.Controls.Add(dataPanel);

            return blockPanel;
        }

        /// <summary>
        /// The tab strip which holds one tab per update source that has data for this mod.
        /// </summary>
        /// <remarks>
        /// The control of each source is built by that source, so this method knows only that
        /// a source hands back a control and a tab caption. What it does own is the wiring:
        /// the source's control says when its data changed, and the block passes that on with
        /// the data, which is what lets the editor store it against the right mod.
        /// </remarks>
        private ThemedTabControl BuildSourceTabs()
        {
            var sourceTabs = new ThemedTabControl
            {
                Margin = new Padding(0),
                Size = new Size(EditorLayout.PanelSeedSize, EditorLayout.PanelSeedSize),
            };

            foreach (var sourceData in _infoData.Sources)
            {
                var infoControl = sourceData.Source.CreateInfoControl(sourceData, _toolTip);
                infoControl.DataChanged += OnSourceDataChanged;

                // Measured before it is docked: a docked control takes the size of the page it
                // is put in, and the page is not laid out until the tab is selected, so what
                // the strip has to cover would be lost.
                var needed = EditorLayout.OccupiedSize(infoControl.Control);

                infoControl.Control.Dock = DockStyle.Fill;

                var sourceTabPage = new TabPage
                {
                    Text = sourceData.Source.Name,
                    // The data is what the block's buttons need to know which source the user
                    // is looking at, and the page is the only thing which knows that.
                    Tag = sourceData,
                };
                sourceTabPage.Controls.Add(infoControl.Control);

                sourceTabs.TabPages.Add(sourceTabPage);

                sourceTabs.Size = new Size(
                    needed.Width + SourceTabWidthReserve > sourceTabs.Width
                        ? needed.Width + SourceTabWidthReserve
                        : sourceTabs.Width,
                    needed.Height + SourceTabHeightReserve > sourceTabs.Height
                        ? needed.Height + SourceTabHeightReserve
                        : sourceTabs.Height);
            }

            return sourceTabs;
        }

        private void OnSourceDataChanged(object sender, EventArgs e)
        {
            var infoControl = sender as UpdateSourceInfoControl;
            if (infoControl == null) return;

            _dataChanged(infoControl.Data);
        }

        private void AddButtons(FlowLayoutPanel dataPanel)
        {
            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                BorderStyle = BorderStyle.Fixed3D,
            };

            foreach (var buttonData in Buttons)
            {
                // no explicit size: the button measures itself from its caption, and a
                // size set on an auto sized button is overwritten on the next layout
                var button = new Button
                {
                    AutoSize = true,
                    Text = buttonData.Text,
                };
                _toolTip.SetToolTip(button, buttonData.Description);
                button.Click += (o, e) =>
                {
                    var target = buttonData.GetProcessStartTarget(_infoData, SelectedSource);
                    if (string.IsNullOrWhiteSpace(target)) return;

                    ManageProcess.SimpleRunProcess(target);
                };
                buttonsPanel.Controls.Add(button);
            }

            dataPanel.Controls.Add(buttonsPanel);

            // measured once, after every button is in. Growing the panel per button
            // counted the same row twice and left the block taller than its content.
            EditorLayout.GrowToCover(dataPanel, EditorLayout.OccupiedSize(buttonsPanel));
        }
    }
}
