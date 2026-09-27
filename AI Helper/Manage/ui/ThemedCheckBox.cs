using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A check box whose box belongs to the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="CheckBox"/> draws the box beside its caption with the system
    /// colours: neither <see cref="Control.BackColor"/> nor
    /// <see cref="FlatButtonAppearance.BorderColor"/> reaches it, and the tick is drawn
    /// in a colour of its own as well. There is no property for any of the three, so the
    /// box is drawn here, over the one the control drew.
    /// </para>
    /// <para>
    /// Only the box is drawn here, and the caption is left to the control. That is what
    /// keeps the caption where it was: the control measures and places it, and it
    /// follows the alignment and the font a designer set.
    /// </para>
    /// <para>
    /// The box drawn here is a pixel wider than the one the control draws, so the
    /// system colours are covered completely rather than left as a sliver along an edge.
    /// </para>
    /// </remarks>
    internal class ThemedCheckBox : CheckBox
    {
        /// <summary>
        /// Side of the box drawn beside the caption.
        /// </summary>
        private const int GlyphSize = 13;

        /// <summary>
        /// Thickness of the tick.
        /// </summary>
        private const float MarkWidth = 2F;

        private Color _glyphFace = SystemColors.Window;
        private Color _glyphBorder = SystemColors.ControlDark;
        private Color _glyphMark = SystemColors.ControlText;

        /// <summary>
        /// Colour the box is filled with, in every state. Set by the theme loader.
        /// </summary>
        internal Color GlyphFace
        {
            get
            {
                return _glyphFace;
            }

            set
            {
                if (_glyphFace == value) return;

                _glyphFace = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Colour of the outline of the box. Set by the theme loader.
        /// </summary>
        internal Color GlyphBorder
        {
            get
            {
                return _glyphBorder;
            }

            set
            {
                if (_glyphBorder == value) return;

                _glyphBorder = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Colour of the tick. Set by the theme loader.
        /// </summary>
        internal Color GlyphMark
        {
            get
            {
                return _glyphMark;
            }

            set
            {
                if (_glyphMark == value) return;

                _glyphMark = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The rectangle the box occupies.
        /// </summary>
        /// <remarks>
        /// A function of the height alone, so the box stays centred on the caption
        /// whatever the font size turns out to be.
        /// </remarks>
        internal static Rectangle GlyphBounds(Size size)
        {
            return new Rectangle(0, Math.Max(0, (size.Height - GlyphSize) / 2), GlyphSize, GlyphSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // the control draws the caption, and the box this replaces
            base.OnPaint(e);

            PaintGlyph(e.Graphics);
        }

        private void PaintGlyph(Graphics graphics)
        {
            var box = GlyphBounds(Size);

            using (var face = new SolidBrush(_glyphFace))
            {
                graphics.FillRectangle(face, box);
            }

            using (var border = new Pen(_glyphBorder))
            {
                graphics.DrawRectangle(border, box.X, box.Y, box.Width - 1, box.Height - 1);
            }

            if (CheckState == CheckState.Unchecked) return;

            // a box which cannot be changed keeps its shape but loses its weight, the
            // same way a disabled caption does
            var markColour = Enabled ? _glyphMark : ThemePalette.Blend(_glyphMark, _glyphFace, 0.55f);

            using (var mark = new Pen(markColour, MarkWidth))
            {
                if (CheckState == CheckState.Indeterminate)
                {
                    var middle = box.Top + (box.Height / 2);

                    graphics.DrawLine(mark, box.Left + 3, middle, box.Right - 4, middle);
                    return;
                }

                graphics.DrawLines(mark, new[]
                {
                    new Point(box.Left + 3, box.Top + 7),
                    new Point(box.Left + 6, box.Top + 10),
                    new Point(box.Right - 3, box.Top + 3)
                });
            }
        }
    }
}
