using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A text box whose frame belongs to the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An edit control with <see cref="BorderStyle.FixedSingle"/> draws its one pixel
    /// frame itself, in a colour taken from the system, and no property of the control
    /// reaches it. The frame is therefore painted over, in
    /// <see cref="WndProc"/>'s handling of the message which asks for it.
    /// </para>
    /// <para>
    /// The frame is in the non client area, so the context it is painted with has to be
    /// one for the whole window - the client one has its origin inside the frame. It is
    /// painted over rather than drawn first, because the control paints its frame when
    /// it is asked to and would cover anything drawn before it. That means one extra
    /// pass, over a single pixel of a control which is not being resized, which is why
    /// this is not the approach the tab strip takes.
    /// </para>
    /// </remarks>
    internal class ThemedTextBox : TextBox
    {
        /// <summary>
        /// Sent when the frame of a window has to be drawn.
        /// </summary>
        private const int WmNcPaint = 0x0085;

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
                RepaintFrame();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WmNcPaint)
            {
                PaintBorder(NativeMethods.GetWindowDC(Handle), true);
            }
            else if ((m.Msg == WmPrint || m.Msg == WmPrintClient) && m.WParam != IntPtr.Zero)
            {
                // the context already has its origin at this window, so the same
                // rectangle is the right one here as well
                PaintBorder(m.WParam, false);
            }
        }

        /// <summary>
        /// The rectangle the frame occupies, in the coordinates of the window.
        /// </summary>
        internal Rectangle BorderBounds()
        {
            return new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
        }

        private void PaintBorder(IntPtr deviceContext, bool owned)
        {
            if (deviceContext == IntPtr.Zero || !IsHandleCreated) return;

            // a box with no frame has no frame to colour, and painting one would draw
            // over the text instead
            if (BorderStyle != BorderStyle.FixedSingle) return;

            try
            {
                using (var graphics = Graphics.FromHdc(deviceContext))
                using (var border = new Pen(_borderColour))
                {
                    graphics.DrawRectangle(border, BorderBounds());
                }
            }
            finally
            {
                if (owned) NativeMethods.ReleaseDC(Handle, deviceContext);
            }
        }

        private void RepaintFrame()
        {
            if (!IsHandleCreated) return;

            NativeMethods.RedrawWindow(
                Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                NativeMethods.RedrawInvalidate | NativeMethods.RedrawFrame);
        }
    }
}
