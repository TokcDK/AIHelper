using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.Update;

namespace AIHelper.Manage.Functions
{
    /// <summary>
    /// The UI block of one mod: its name, one editor per github field and the buttons
    /// which open it.
    /// </summary>
    /// <remarks>
    /// A block is built once per editor session and then reused, so the editor is not
    /// rebuilt every time the filter runs. <see cref="Filter"/> is the text the block is
    /// matched against.
    /// </remarks>
    internal sealed class ModInfoBlock
    {
        /// <summary>Width of the caption of a field.</summary>
        private const int FieldLabelWidth = 150;

        /// <summary>Width of the text box of a text field.</summary>
        private const int FieldBoxWidth = 200;

        /// <summary>Width of the check box of a boolean field.</summary>
        private const int FieldCheckBoxWidth = 300;

        /// <summary>Width of the caption of the block, which holds the mod name.</summary>
        private const int BlockLabelWidth = 100;

        /// <summary>Size a flow panel starts at before its content widens it.</summary>
        private const int PanelSeedSize = 10;

        /// <summary>Room the data panel keeps around the fields it holds.</summary>
        private const int DataPanelExtraWidth = 10;

        /// <inheritdoc cref="DataPanelExtraWidth"/>
        private const int DataPanelExtraHeight = 15;

        /// <summary>Room the block keeps around the mod name and the data panel.</summary>
        private const int BlockExtraSize = 15;

        /// <summary>Height the block is pinned to once its content has been measured.</summary>
        private const int BlockHeight = 300;

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
        private readonly int _elementHeight;
        private readonly Action<Binding, UpdateInfoData> _writeValue;

        /// <param name="elementHeight">Height of a single row of controls.</param>
        /// <param name="writeValue">
        /// Called with the binding of the field the user edited, so that the editor can
        /// store it once the edits have settled.
        /// </param>
        internal ModInfoBlock(UpdateInfoData infoData, ToolTip toolTip, int elementHeight,
            Action<Binding, UpdateInfoData> writeValue)
        {
            _infoData = infoData;
            _toolTip = toolTip;
            _elementHeight = elementHeight;
            _writeValue = writeValue;

            Control = Build();
        }

        /// <summary>The control element with all added mod info elements.</summary>
        internal Control Control { get; }

        internal UpdateInfoData InfoData => _infoData;

        /// <summary>
        /// Getter-only text used to filter the mod: merged "Mod dir name", "url",
        /// github "Owner" and github "repository".
        /// </summary>
        internal string Filter =>
            string.Join(" ", new[] { _infoData.ModName, Url, _infoData.GitInfo.Owner, _infoData.GitInfo.Repository });

        private string Url => GithubUrl.BuildRepositoryUrl(
            _infoData.GitInfo.Site, _infoData.GitInfo.Owner, _infoData.GitInfo.Repository);

        /// <summary>
        /// The size a control occupies, its margins included.
        /// </summary>
        /// <remarks>
        /// The margins are counted twice, as the hand tuned layout of this window always
        /// did. The numbers this produces are the ones the blocks were sized with, so it
        /// is left alone.
        /// </remarks>
        private static Size OccupiedSize(Control control)
        {
            return new Size(
                control.Width + (control.Margin.Horizontal * 2),
                control.Height + (control.Margin.Vertical * 2));
        }

        /// <summary>
        /// Widens a panel when the content is wider than it, and makes it tall enough for
        /// the content.
        /// </summary>
        private static void GrowToCover(FlowLayoutPanel panel, Size content)
        {
            panel.Size = new Size(
                content.Width > panel.Width ? content.Width : panel.Width,
                panel.Height + content.Height);
        }

        /// <summary>
        /// Widens and heightens a panel to cover the content, never shrinking it.
        /// </summary>
        private static void GrowToCoverBoth(FlowLayoutPanel panel, Size content)
        {
            panel.Size = new Size(
                content.Width > panel.Width ? content.Width : panel.Width,
                content.Height > panel.Height ? content.Height : panel.Height);
        }

        private FlowLayoutPanel Build()
        {
            var blockPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                Size = new Size(PanelSeedSize, PanelSeedSize),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.Fixed3D,
            };

            var nameLabel = new Label
            {
                AutoSize = true,
                Text = $"{T._("Name")}: {_infoData.ModName}",
                Size = new Size(BlockLabelWidth, _elementHeight * 3),
                Margin = new Padding(0),
            };
            _toolTip.SetToolTip(nameLabel, T._("Mod dir name"));
            blockPanel.Controls.Add(nameLabel);

            GrowToCoverBoth(blockPanel, OccupiedSize(nameLabel));

            var dataPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Size = new Size(PanelSeedSize, PanelSeedSize),
                Margin = new Padding(0),
                BorderStyle = BorderStyle.Fixed3D,
            };

            AddGithubData(dataPanel);
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

        private void AddGithubData(FlowLayoutPanel dataPanel)
        {
            var hostLabel = new Label
            {
                Text = T._("Github"),
                Size = new Size(BlockLabelWidth, _elementHeight * 3),
                Margin = new Padding(0),
            };
            dataPanel.Controls.Add(hostLabel);

            GrowToCoverBoth(dataPanel, OccupiedSize(hostLabel));

            foreach (var property in _infoData.GitInfo.GetType().GetProperties())
            {
                // A property the user cannot write (Site, INI, Mod) is not a field, and a
                // property the editor has no caption for is not one it knows how to show.
                // Both are skipped rather than guessed at.
                if (!property.CanWrite) continue;

                string caption;
                string description;
                if (!GitUpdateInfoData.TryGetFieldText(property.Name, out caption, out description)) continue;

                Control field;
                if (property.PropertyType == typeof(string))
                {
                    field = CreateTextField(property.Name, caption, description);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    field = CreateCheckBoxField(property.Name, caption, description);
                }
                else continue;

                dataPanel.Controls.Add(field);

                GrowToCover(dataPanel, OccupiedSize(field));
            }
        }

        /// <summary>
        /// One text field of the block: a caption and the text box which edits the value.
        /// </summary>
        private Control CreateTextField(string propertyName, string caption, string description)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
            };

            var label = new Label
            {
                Text = caption + ":",
                Size = new Size(FieldLabelWidth, _elementHeight),
                Margin = new Padding(0),
            };

            var textBox = new TextBox
            {
                Size = new Size(FieldBoxWidth, _elementHeight),
                Margin = new Padding(0),
            };

            var binding = BindField(textBox, "Text", propertyName);
            textBox.TextChanged += (o, e) => _writeValue(binding, _infoData);

            row.Controls.Add(label);
            row.Controls.Add(textBox);

            _toolTip.SetToolTip(label, description);
            _toolTip.SetToolTip(textBox, description);

            var labelSize = OccupiedSize(label);
            var boxSize = OccupiedSize(textBox);
            row.Size = new Size(labelSize.Width + boxSize.Width, labelSize.Height + boxSize.Height);

            return row;
        }

        /// <summary>
        /// One boolean field of the block: a check box which edits the value.
        /// </summary>
        private Control CreateCheckBoxField(string propertyName, string caption, string description)
        {
            var checkBox = new CheckBox
            {
                Text = caption,
                Size = new Size(FieldCheckBoxWidth, _elementHeight + 10),
                Margin = new Padding(0),
            };

            var binding = BindField(checkBox, "Checked", propertyName);
            checkBox.CheckedChanged += (o, e) => _writeValue(binding, _infoData);

            _toolTip.SetToolTip(checkBox, description);

            return checkBox;
        }

        /// <summary>
        /// Connects a control to one property of the block's coordinates.
        /// </summary>
        /// <returns>
        /// The binding, so that the field can push the edited value into the coordinates
        /// without looking the binding up again by index.
        /// </returns>
        private Binding BindField(Control control, string controlProperty, string propertyName)
        {
            var binding = new Binding(
                controlProperty,
                _infoData,
                $"{nameof(UpdateInfoData.GitInfo)}.{propertyName}",
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            control.DataBindings.Add(binding);

            return binding;
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
                    var target = buttonData.GetProcessStartTarget(_infoData);
                    if (string.IsNullOrWhiteSpace(target)) return;

                    ManageProcess.SimpleRunProcess(target);
                };
                buttonsPanel.Controls.Add(button);
            }

            dataPanel.Controls.Add(buttonsPanel);

            // measured once, after every button is in. Growing the panel per button
            // counted the same row twice and left the block taller than its content.
            GrowToCover(dataPanel, OccupiedSize(buttonsPanel));
        }
    }
}
