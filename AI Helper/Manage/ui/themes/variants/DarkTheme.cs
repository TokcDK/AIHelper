using System.Drawing;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Dark theme: the neutral greys and the blue accent of a modern dark editor.
    /// </summary>
    internal class DarkTheme : IUITheme
    {
        public string Name { get { return "Dark"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
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

                Accent = Color.White,
                AccentText = Color.FromArgb(255, 255, 255, 255),

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

                TabFace = Color.WhiteSmoke,
                TabSelectedFace = Color.White,
                TabText = Color.DarkGray,
                TabSelectedText = Color.Black,

                Link = Color.FromArgb(255, 79, 163, 255),
                Selection = Color.FromArgb(255, 9, 71, 113),
                SelectionText = Color.FromArgb(255, 255, 255, 255),

                ProgressBar = Color.FromArgb(255, 55, 148, 255),
            };
        }
    }
}
