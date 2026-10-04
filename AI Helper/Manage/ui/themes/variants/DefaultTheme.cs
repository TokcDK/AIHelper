using System.Drawing;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui.themes.variants
{
    /// <summary>
    /// Light theme the application starts with: white content surfaces, a slate
    /// title bar and a Windows blue accent.
    /// </summary>
    internal class DefaultTheme : IUITheme
    {
        public string Name { get { return "Default"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            return new ThemePalette
            {
                IsDark = false,

                Window = Color.FromArgb(255, 240, 242, 245),
                Surface = Color.FromArgb(255, 255, 255, 255),
                SurfaceAlt = Color.FromArgb(255, 246, 248, 250),
                Border = Color.FromArgb(255, 214, 219, 225),

                Text = Color.FromArgb(255, 32, 37, 43),
                TextMuted = Color.FromArgb(255, 104, 114, 125),

                TitleBar = Color.FromArgb(255, 43, 49, 56),
                TitleBarText = Color.FromArgb(255, 245, 247, 250),
                TitleBarMuted = Color.FromArgb(255, 154, 164, 173),

                Accent = Color.FromArgb(255, 0, 120, 212),
                AccentText = Color.FromArgb(255, 255, 255, 255),

                ButtonFace = Color.FromArgb(255, 251, 252, 253),
                ButtonText = Color.FromArgb(255, 32, 37, 43),
                ButtonHover = Color.FromArgb(255, 233, 242, 251),
                ButtonPressed = Color.FromArgb(255, 214, 233, 250),
                ButtonBorder = Color.FromArgb(255, 205, 211, 218),

                Danger = Color.FromArgb(255, 196, 43, 28),
                DangerText = Color.FromArgb(255, 255, 255, 255),

                InputFace = Color.FromArgb(255, 255, 255, 255),
                InputText = Color.FromArgb(255, 32, 37, 43),
                InputBorder = Color.FromArgb(255, 198, 204, 211),

                TabFace = Color.FromArgb(255, 235, 238, 242),
                TabSelectedFace = Color.FromArgb(255, 255, 255, 255),
                TabText = Color.FromArgb(255, 96, 106, 116),
                TabSelectedText = Color.FromArgb(255, 32, 37, 43),

                Link = Color.FromArgb(255, 0, 102, 204),
                Selection = Color.FromArgb(255, 204, 228, 247),
                SelectionText = Color.FromArgb(255, 16, 30, 42),

                ProgressBar = Color.FromArgb(255, 0, 120, 212),
            }.Complete();
        }
    }
}
