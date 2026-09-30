using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// Gives a tool strip the colours of the theme.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A tool strip is drawn by a renderer rather than by the control itself, and the
    /// renderer the application ends up with is the professional one, which takes every
    /// colour it uses from a colour table of its own. The strip's own BackColor and
    /// ForeColor are ignored because of that: the status bar of the updater window stayed
    /// the system grey under a dark window, and its caption stayed black.
    /// </para>
    /// <para>
    /// The colours are handed over by replacing the renderer, which is the only lever a
    /// tool strip offers for this. The replacement is the professional renderer with a
    /// colour table built from the palette, so the face, the border and the entries all
    /// follow the theme while the layout and the behaviour stay the ones the control
    /// already had.
    /// </para>
    /// <para>
    /// Applying a theme is safe to repeat: every pass puts a fresh renderer in place and
    /// nothing accumulates. The renderer is replaced rather than updated because it reads
    /// the palette it was built with, and a palette is not something a renderer can be
    /// told to change afterwards.
    /// </para>
    /// </remarks>
    internal static class ThemedToolStrip
    {
        /// <summary>
        /// Draws <paramref name="toolStrip"/> in the colours of <paramref name="palette"/>.
        /// </summary>
        internal static void Apply(ToolStrip toolStrip, ThemePalette palette)
        {
            if (toolStrip == null || palette == null) return;

            toolStrip.Renderer = new ThemedRenderer(palette);

            // the caption of an entry is taken from the strip when the entry does not set
            // one of its own, so the text colour has to be set here to reach the captions
            toolStrip.ForeColor = palette.Text;
            toolStrip.BackColor = palette.Surface;
        }

        /// <summary>
        /// Draws a tool strip from a palette.
        /// </summary>
        private sealed class ThemedRenderer : ToolStripProfessionalRenderer
        {
            private readonly ThemePalette _palette;

            internal ThemedRenderer(ThemePalette palette)
                : base(new ThemedColourTable(palette))
            {
                _palette = palette;

                // the strip is a flat face, so the rounded ends the professional renderer
                // adds to make it look raised are not wanted
                RoundedEdges = false;
            }

            /// <summary>
            /// Paints the face of the strip.
            /// </summary>
            /// <remarks>
            /// The colour table is consulted for a gradient rather than for a single colour,
            /// and the renderer draws the strip in bands, so a gradient whose three colours
            /// are the same still comes out with a seam where the bands meet. Filling the
            /// face here is what makes the strip one flat surface.
            /// </remarks>
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var brush = new SolidBrush(_palette.Surface))
                {
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
                }
            }
        }

        /// <summary>
        /// The colours the professional renderer draws a tool strip with.
        /// </summary>
        /// <remarks>
        /// Only the roles this application can actually reach are overridden. The rest keep
        /// the values the base class derives, so a menu or a drop down added later is drawn
        /// with the system colours rather than not at all - it is then one more control to
        /// bring into the theme, not a hole in the window.
        /// </remarks>
        private sealed class ThemedColourTable : ProfessionalColorTable
        {
            private readonly ThemePalette _palette;

            internal ThemedColourTable(ThemePalette palette)
            {
                _palette = palette;
            }

            public override Color ToolStripBorder { get { return _palette.Border; } }

            // the face of a status bar is drawn in two bands, so both have to be the same
            // colour for the strip to come out flat
            public override Color StatusStripGradientBegin { get { return _palette.Surface; } }
            public override Color StatusStripGradientEnd { get { return _palette.Surface; } }

            // the grip in the corner of a status bar is drawn in two shades of its own
            public override Color GripDark { get { return _palette.Border; } }
            public override Color GripLight { get { return _palette.Surface; } }

            public override Color ToolStripDropDownBackground { get { return _palette.SurfaceAlt; } }
            public override Color ImageMarginGradientBegin { get { return _palette.SurfaceAlt; } }
            public override Color ImageMarginGradientMiddle { get { return _palette.SurfaceAlt; } }
            public override Color ImageMarginGradientEnd { get { return _palette.SurfaceAlt; } }

            public override Color MenuBorder { get { return _palette.Border; } }
            public override Color MenuItemBorder { get { return _palette.Accent; } }
            public override Color MenuItemSelected { get { return _palette.ButtonHover; } }
            public override Color MenuItemSelectedGradientBegin { get { return _palette.ButtonHover; } }
            public override Color MenuItemSelectedGradientEnd { get { return _palette.ButtonHover; } }
        }
    }
}
