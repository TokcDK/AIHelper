using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A tab strip which the theme paints in full.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="TabControl"/> draws the area behind its headers itself and ignores
    /// <see cref="Control.BackColor"/> there, so the strip stayed the system colour
    /// whatever theme was applied: the headers were themed and the area around them was
    /// not. The same control draws a two pixel frame around every header in a system
    /// colour, which is what the light line around a tab was, and it has no property for
    /// that either. Both are painted here instead.
    /// </para>
    /// <para>
    /// The paint is taken over rather than run afterwards, and that is the second reason
    /// this class exists. Painting after the control had painted meant the system
    /// coloured strip was on screen until the second pass reached it, which is what
    /// flickered while the window was being resized: every resize step showed the strip
    /// twice, once in the wrong colour. <see cref="ControlStyles.UserPaint"/> puts the
    /// whole strip into one pass, and <see cref="ControlStyles.OptimizedDoubleBuffer"/>
    /// keeps that pass off the screen until it is finished, so there is no intermediate
    /// state left to see.
    /// </para>
    /// <para>
    /// Taking the paint over also means the control no longer draws its own headers, so
    /// the theme draws them, through <see cref="ThemesLoader.PaintTabHeader"/>. That is
    /// the same method the owner drawing used to call, so a header looks the same as it
    /// did.
    /// </para>
    /// </remarks>
    internal class ThemedTabControl : TabControl
    {
        private Color _stripColor = SystemColors.Control;

        /// <remarks>
        /// Public although the class is not, which is what a class with no constructor of
        /// its own would have been given anyway. Anything which builds a control from its
        /// type - the form designer, or a test which reflects over the assembly - looks for
        /// a public one and finds nothing otherwise.
        /// </remarks>
        public ThemedTabControl()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);
        }

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

        /// <summary>
        /// Paints the strip and then the headers on it.
        /// </summary>
        /// <remarks>
        /// The pages are windows of their own and are painted by themselves, so they are
        /// left alone. Everything else in the control is the strip.
        /// </remarks>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (TabCount == 0) return;

            var graphics = e.Graphics;

            using (var strip = new Region(ClientRectangle))
            {
                strip.Exclude(DisplayRectangle);

                using (var brush = new SolidBrush(_stripColor))
                {
                    graphics.FillRegion(brush, strip);
                }
            }

            for (var index = 0; index < TabCount; index++)
            {
                ThemesLoader.PaintTabHeader(graphics, this, index);
            }
        }
    }
}
