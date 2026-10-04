using System.Drawing;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui.themes.variants
{
    /// <summary>
    /// Dark theme: the neutral greys and the blue accent of a modern dark editor.
    /// </summary>
    /// <remarks>
    /// The palette ends in <see cref="ThemePalette.Complete"/> like every other theme.
    /// It used to be the one theme which did not, and the roles it therefore left empty
    /// - the hover shades, the disabled text and the progress bar track - fell back to
    /// the WinForms defaults instead of following the theme.
    /// </remarks>
    internal class DarkTheme : IUITheme
    {
        public string Name { get { return "Dark"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            // one blue for the accent and the progress bar, so the two cannot drift
            var accent = Color.FromArgb(255, 55, 148, 255);

            return new ThemePalette
            {
                IsDark = true,

                Window = Color.FromArgb(255, 30, 30, 30),
                Surface = Color.FromArgb(255, 37, 37, 38),
                SurfaceAlt = Color.FromArgb(255, 45, 45, 48),
                Border = Color.FromArgb(255, 63, 63, 70),

                Text = Color.White,
                TextMuted = Color.DarkGray,

                TitleBar = Color.FromArgb(255, 22, 25, 28),
                TitleBarText = Color.FromArgb(255, 240, 242, 244),
                TitleBarMuted = Color.FromArgb(255, 140, 148, 156),

                // a white accent made a primary button white on white: the face and the
                // text on it have to differ, and this is the blue the theme describes
                Accent = accent,
                AccentText = Color.White,

                ButtonFace = Color.FromArgb(255, 51, 51, 55),
                ButtonText = Color.FromArgb(255, 232, 232, 232),
                ButtonHover = Color.FromArgb(255, 63, 63, 68),
                ButtonPressed = Color.FromArgb(255, 43, 43, 47),
                ButtonBorder = Color.FromArgb(255, 90, 90, 95),

                Danger = Color.FromArgb(255, 196, 43, 28),
                DangerText = Color.FromArgb(255, 255, 255, 255),

                InputFace = Color.FromArgb(255, 42, 42, 44),
                InputText = Color.FromArgb(255, 232, 232, 232),
                InputBorder = Color.FromArgb(255, 70, 70, 75),

                // the strip is the window chrome behind the headers, and the selected
                // header is the surface the page below it is drawn on, so the two read as
                // one piece. These were light, which left a bright band across the top of
                // a dark window.
                TabFace = Color.FromArgb(255, 30, 30, 30),
                TabSelectedFace = Color.FromArgb(255, 37, 37, 38),
                TabText = Color.FromArgb(255, 160, 160, 165),
                TabSelectedText = Color.White,

                Link = Color.FromArgb(255, 79, 163, 255),
                Selection = Color.FromArgb(255, 9, 71, 113),
                SelectionText = Color.FromArgb(255, 255, 255, 255),

                ProgressBar = accent,
            }.Complete();
        }
    }
}
