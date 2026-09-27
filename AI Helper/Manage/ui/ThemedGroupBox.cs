using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A group box whose frame belongs to the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="GroupBox"/> draws the frame around itself, and the colour it uses is
    /// a system one: setting <see cref="Control.ForeColor"/> colours the caption and
    /// leaves the frame exactly as it was. So the frame is drawn here instead, together
    /// with the face and the caption, which is everything a group box is made of.
    /// </para>
    /// <para>
    /// The geometry is the framework's own - the frame is inset by one pixel, and the
    /// line the caption sits on runs through the middle of the caption's height - so a
    /// group box does not move or change size when it is drawn this way. The caption
    /// interrupts the frame line behind it, which is what makes it readable.
    /// </para>
    /// </remarks>
    internal class ThemedGroupBox : GroupBox
    {
        /// <summary>
        /// Distance from the left edge to the caption.
        /// </summary>
        private const int CaptionLeft = 8;

        private Color _borderColour = SystemColors.ControlDark;

        /// <summary>
        /// Colour of the frame. Set by the theme loader.
        /// </summary>
        internal Color BorderColour
        {
            get
            {
                return _borderColour;
            }

            set
            {
                if (_borderColour == value) return;

                _borderColour = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Distance from the top of the control to the line the frame runs along.
        /// </summary>
        internal static int FrameTop(int fontHeight)
        {
            return fontHeight / 2;
        }

        /// <summary>
        /// The rectangle the frame is drawn on.
        /// </summary>
        internal static Rectangle FrameBounds(Size size, int top)
        {
            return new Rectangle(0, top, Math.Max(0, size.Width - 1), Math.Max(0, size.Height - top - 1));
        }

        /// <summary>
        /// The rectangle the caption is drawn in, centred on the frame line.
        /// </summary>
        internal static Rectangle CaptionBounds(int top, Size caption)
        {
            return new Rectangle(CaptionLeft, top - (caption.Height / 2), caption.Width, caption.Height);
        }

        /// <summary>
        /// Draws the face, the frame and the caption.
        /// </summary>
        /// <remarks>
        /// The framework is not asked to draw first: it would draw its own frame, in the
        /// colour this class exists to replace, and the two frames would have to line up
        /// exactly to cover it.
        /// </remarks>
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var top = FrameTop(Font.Height);

            using (var face = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(face, ClientRectangle);
            }

            using (var border = new Pen(_borderColour))
            {
                graphics.DrawRectangle(border, FrameBounds(ClientSize, top));
            }

            PaintCaption(graphics, top);
        }

        private void PaintCaption(Graphics graphics, int top)
        {
            if (string.IsNullOrEmpty(Text)) return;

            var caption = TextRenderer.MeasureText(
                Text,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding);

            var bounds = CaptionBounds(top, caption);

            // the face colour is put back behind the caption, which is what breaks the
            // frame line there and keeps the text off it
            using (var face = new SolidBrush(BackColor))
            {
                graphics.FillRectangle(face, bounds);
            }

            TextRenderer.DrawText(graphics, Text, Font, bounds, ForeColor, TextFormatFlags.NoPadding);
        }
    }
}
