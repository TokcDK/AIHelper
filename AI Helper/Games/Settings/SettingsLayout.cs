using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui;

namespace AIHelper.Games.Settings
{
    /// <summary>
    /// Builds the controls a game settings tab is made of, so both tabs look and
    /// behave the same without each of them repeating the layout code.
    /// </summary>
    internal static class SettingsLayout
    {
        private const float CaptionColumnPercent = 38f;
        private const float EditorColumnPercent = 62f;

        /// <summary>
        /// Creates the two column grid the rows are put into. The first column
        /// holds the captions, the second one the editors.
        /// </summary>
        internal static TableLayoutPanel CreateTable(int rowCount)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = rowCount,
                Padding = new Padding(8),
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, CaptionColumnPercent));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, EditorColumnPercent));

            for (var i = 0; i < rowCount; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rowCount));
            }

            return table;
        }

        /// <summary>
        /// Adds a captioned editor to one row of the grid.
        /// </summary>
        internal static void AddRow(TableLayoutPanel table, int row, string caption, Control editor)
        {
            table.Controls.Add(CreateCaption(caption), 0, row);
            table.Controls.Add(editor, 1, row);
        }

        /// <summary>
        /// Creates a right aligned caption for the first column.
        /// </summary>
        internal static Label CreateCaption(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Margin = new Padding(2),
        };

        /// <summary>
        /// Creates a closed drop down list.
        /// </summary>
        internal static ComboBox CreateDropDown(string[] items, EventHandler onSelectionChanged)
        {
            var dropDown = new ThemedComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(2),
            };

            dropDown.Items.AddRange(items);
            dropDown.SelectedIndexChanged += onSelectionChanged;

            return dropDown;
        }

        /// <summary>
        /// Creates a check box without a caption, the caption is the row caption.
        /// </summary>
        internal static CheckBox CreateCheckBox(EventHandler onCheckedChanged)
        {
            var checkBox = new ThemedCheckBox
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(2),
            };

            checkBox.CheckedChanged += onCheckedChanged;

            return checkBox;
        }

        /// <summary>
        /// Creates a link that runs <paramref name="onClick"/> when it is clicked.
        /// </summary>
        internal static LinkLabel CreateLink(string text, EventHandler onClick)
        {
            var link = new LinkLabel
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(2),
            };

            link.Click += onClick;

            return link;
        }
    }
}
