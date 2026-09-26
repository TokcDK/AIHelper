using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A tab strip whose background belongs to the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="TabControl"/> draws the area behind its headers itself and ignores
    /// <see cref="Control.BackColor"/> there, so the strip stayed the system colour
    /// whatever theme was applied: the headers were themed and the area around them
    /// was not. There is no property which reaches that area, so it is painted here
    /// instead.
    /// </para>
    /// <para>
    /// The paint runs after the control has drawn itself and fills everything which is
    /// not a page and not a tab header - that is, the band above the pages, the thin
    /// frame around them and the empty space beside the last tab. Painting the gaps
    /// rather than the whole strip is what keeps the headers intact: they are drawn by
    /// the theme's own owner drawing, and covering them would mean drawing them twice.
    /// </para>
    /// </remarks>
    internal class ThemedTabControl : TabControl
    {
        private const int WmPaint = 0x000F;

        /// <summary>
        /// Sent when a control is asked to draw itself into a device context it did not
        /// get from the window - a screenshot, a print, or <c>DrawToBitmap</c>. The strip
        /// has to be painted there as well, or the control renders with the very
        /// background this class exists to replace.
        /// </summary>
        private const int WmPrint = 0x0317;

        /// <summary>
        /// The same request without the non-client frame. Both arrive here: a window is
        /// printed with <see cref="WmPrint"/> and a child control with
        /// <see cref="WmPrintClient"/>, and <c>DrawToBitmap</c> uses the first.
        /// </summary>
        private const int WmPrintClient = 0x0318;

        private Color _stripColor = SystemColors.Control;

        /// <summary>
        /// Colour painted behind the tab headers. Set by the theme loader.
        /// </summary>
        internal Color StripColor
        {
            get
            {
                return _stripColor;
            }

            set
            {
                if (_stripColor == value) return;

                _stripColor = value;
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
                    PaintStrip(graphics);
                }
            }
            else if ((m.Msg == WmPrint || m.Msg == WmPrintClient) && m.WParam != IntPtr.Zero)
            {
                // the device context already has its origin at this control, so the
                // client rectangle is the right place to paint in either case
                using (var graphics = Graphics.FromHdc(m.WParam))
                {
                    PaintStrip(graphics);
                }
            }
        }

        /// <summary>
        /// Fills the part of the control which neither a page nor a tab header covers.
        /// </summary>
        private void PaintStrip(Graphics graphics)
        {
            if (!IsHandleCreated || TabCount == 0) return;

            using (var strip = new Region(ClientRectangle))
            {
                // the pages are drawn by their own windows, so they are not ours to paint
                strip.Exclude(DisplayRectangle);

                // the headers are drawn by the theme's owner drawing, so they are left
                // exactly as they were painted
                for (var index = 0; index < TabCount; index++)
                {
                    strip.Exclude(GetTabRect(index));
                }

                using (var brush = new SolidBrush(_stripColor))
                {
                    graphics.FillRegion(brush, strip);
                }
            }
        }
    }
}
