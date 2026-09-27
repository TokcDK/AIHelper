using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A drop down whose outline belongs to the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A drop down drawn flat paints its own one pixel outline, in a colour taken from
    /// the system, and it has no property for that colour. Unlike a text box it has no
    /// non client area either - the outline is inside the client area - so it cannot be
    /// painted through the frame message and is painted over after the control has drawn
    /// instead.
    /// </para>
    /// <para>
    /// That is one extra pass, over a single pixel. The strip of the tab control used to
    /// be painted the same way and flickered while the window was resized, which is why
    /// it does not any more; a one pixel outline of a control which is not being resized
    /// does not show that, and there is no other way to reach it.
    /// </para>
    /// </remarks>
    internal class ThemedComboBox : ComboBox
    {
        /// <summary>
        /// Sent when a window has to draw itself.
        /// </summary>
        private const int WmPaint = 0x000F;

        /// <summary>
        /// Sent when a window is asked to draw itself into a context it did not get from
        /// the window - a screenshot, a print, or <c>DrawToBitmap</c>.
        /// </summary>
        private const int WmPrint = 0x0317;

        /// <summary>
        /// The same request without the frame. Both arrive here: a window is printed with
        /// <see cref="WmPrint"/> and a child control with <see cref="WmPrintClient"/>.
        /// </summary>
        private const int WmPrintClient = 0x0318;

        private Color _borderColour = SystemColors.WindowFrame;

        /// <summary>
        /// Colour of the outline. Set by the theme loader.
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

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WmPaint)
            {
                using (var graphics = Graphics.FromHwnd(Handle))
                {
                    PaintBorder(graphics);
                }
            }
            else if ((m.Msg == WmPrint || m.Msg == WmPrintClient) && m.WParam != IntPtr.Zero)
            {
                using (var graphics = Graphics.FromHdc(m.WParam))
                {
                    PaintBorder(graphics);
                }
            }
        }

        /// <summary>
        /// The rectangle the outline occupies, in the coordinates of the control.
        /// </summary>
        internal Rectangle BorderBounds()
        {
            return new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
        }

        private void PaintBorder(Graphics graphics)
        {
            if (graphics == null || !IsHandleCreated) return;

            using (var border = new Pen(_borderColour))
            {
                graphics.DrawRectangle(border, BorderBounds());
            }
        }
    }
}
