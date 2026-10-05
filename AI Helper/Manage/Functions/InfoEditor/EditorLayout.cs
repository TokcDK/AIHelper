using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The measurements and the flow panel arithmetic every control of the mod info editor
    /// is laid out with.
    /// </summary>
    /// <remarks>
    /// The numbers are the hand tuned ones this window has always used. They live here
    /// rather than in the control which happens to need them, because the mod block, the
    /// source info control and the add-mod form all measure themselves with the same ones
    /// and three copies of "a row is thirteen pixels high" is three chances for the window
    /// to come out uneven.
    /// <para>
    /// <see cref="OccupiedSize"/> counts a control's margins twice, as this layout always
    /// did. The sizes the blocks are given come from those numbers, so the arithmetic is
    /// reproduced rather than corrected: fixing it would move a layout which is not what
    /// this change is about.
    /// </para>
    /// </remarks>
    internal static class EditorLayout
    {
        /// <summary>Height of a single row of controls.</summary>
        internal const int ElementHeight = 13;

        /// <summary>Width of the caption of a field.</summary>
        internal const int FieldLabelWidth = 150;

        /// <summary>Width of the text box of a text field.</summary>
        internal const int FieldBoxWidth = 200;

        /// <summary>Width of the check box of a boolean field.</summary>
        internal const int FieldCheckBoxWidth = 300;

        /// <summary>Size a flow panel starts at before its content widens it.</summary>
        internal const int PanelSeedSize = 10;

        /// <summary>
        /// The size a control occupies, its margins included.
        /// </summary>
        internal static Size OccupiedSize(Control control)
        {
            return new Size(
                control.Width + control.Margin.Horizontal * 2,
                control.Height + control.Margin.Vertical * 2);
        }

        /// <summary>
        /// Widens a panel when the content is wider than it, and makes it tall enough for
        /// the content.
        /// </summary>
        internal static void GrowToCover(FlowLayoutPanel panel, Size content)
        {
            panel.Size = new Size(
                content.Width > panel.Width ? content.Width : panel.Width,
                panel.Height + content.Height);
        }

        /// <summary>
        /// Widens and heightens a panel to cover the content, never shrinking it.
        /// </summary>
        internal static void GrowToCoverBoth(FlowLayoutPanel panel, Size content)
        {
            panel.Size = new Size(
                content.Width > panel.Width ? content.Width : panel.Width,
                content.Height > panel.Height ? content.Height : panel.Height);
        }

        /// <summary>
        /// One row of a control: the caption of a field and the text box which edits it.
        /// </summary>
        /// <param name="caption">Caption shown before the box, without its colon.</param>
        /// <param name="description">Explanation shown as a tooltip over both, or null.</param>
        /// <param name="toolTip">Tooltip the explanation is registered with, or null.</param>
        /// <param name="textBox">Box the value is edited in.</param>
        internal static FlowLayoutPanel CreateFieldRow(string caption, string description,
            ToolTip toolTip, out TextBox textBox)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
            };

            var label = new Label
            {
                Text = caption + ":",
                Size = new Size(FieldLabelWidth, ElementHeight),
                Margin = new Padding(0),
            };

            textBox = new TextBox
            {
                Size = new Size(FieldBoxWidth, ElementHeight),
                Margin = new Padding(0),
            };

            row.Controls.Add(label);
            row.Controls.Add(textBox);

            if (toolTip != null && !string.IsNullOrWhiteSpace(description))
            {
                toolTip.SetToolTip(label, description);
                toolTip.SetToolTip(textBox, description);
            }

            var labelSize = OccupiedSize(label);
            var boxSize = OccupiedSize(textBox);
            row.Size = new Size(labelSize.Width + boxSize.Width, labelSize.Height + boxSize.Height);

            return row;
        }

        /// <summary>
        /// One boolean field: the check box which edits the value.
        /// </summary>
        /// <param name="caption">Caption shown beside the box.</param>
        /// <param name="description">Explanation shown as a tooltip, or null.</param>
        /// <param name="toolTip">Tooltip the explanation is registered with, or null.</param>
        internal static CheckBox CreateCheckBoxField(string caption, string description, ToolTip toolTip)
        {
            var checkBox = new CheckBox
            {
                Text = caption,
                Size = new Size(FieldCheckBoxWidth, ElementHeight + 10),
                Margin = new Padding(0),
            };

            if (toolTip != null && !string.IsNullOrWhiteSpace(description))
            {
                toolTip.SetToolTip(checkBox, description);
            }

            return checkBox;
        }
    }
}
