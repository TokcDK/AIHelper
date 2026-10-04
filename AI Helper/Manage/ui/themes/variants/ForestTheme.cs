using System.Drawing;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui.themes.variants
{
    /// <summary>
    /// Green theme: pale green surfaces with a deep forest accent.
    /// </summary>
    /// <remarks>
    /// The old version of this theme drew black labels on a dark green panel, which
    /// was barely readable. The surfaces are light now and the dark green moved to
    /// the title bar and the accent, where white text sits on it.
    /// </remarks>
    internal class ForestTheme : IUITheme
    {
        public string Name { get { return "Forest"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            return new ThemePalette
            {
                IsDark = false,

                Window = Color.FromArgb(255, 236, 244, 232),
                Surface = Color.FromArgb(255, 247, 251, 245),
                SurfaceAlt = Color.FromArgb(255, 228, 239, 222),
                Border = Color.FromArgb(255, 193, 214, 182),

                Text = Color.FromArgb(255, 27, 46, 23),
                TextMuted = Color.FromArgb(255, 79, 107, 72),

                TitleBar = Color.FromArgb(255, 46, 91, 42),
                TitleBarText = Color.FromArgb(255, 242, 248, 239),
                TitleBarMuted = Color.FromArgb(255, 183, 209, 175),

                Accent = Color.FromArgb(255, 62, 125, 50),
                AccentText = Color.FromArgb(255, 255, 255, 255),

                ButtonFace = Color.FromArgb(255, 232, 241, 226),
                ButtonText = Color.FromArgb(255, 27, 46, 23),
                ButtonHover = Color.FromArgb(255, 216, 232, 206),
                ButtonPressed = Color.FromArgb(255, 198, 221, 185),
                ButtonBorder = Color.FromArgb(255, 187, 211, 174),

                InputFace = Color.FromArgb(255, 255, 255, 255),
                InputText = Color.FromArgb(255, 27, 46, 23),
                InputBorder = Color.FromArgb(255, 187, 211, 174),

                TabFace = Color.FromArgb(255, 228, 239, 222),
                TabSelectedFace = Color.FromArgb(255, 247, 251, 245),
                TabText = Color.FromArgb(255, 79, 107, 72),
                TabSelectedText = Color.FromArgb(255, 27, 46, 23),

                Link = Color.FromArgb(255, 46, 125, 50),
                Selection = Color.FromArgb(255, 203, 227, 191),
                SelectionText = Color.FromArgb(255, 16, 36, 12),

                ProgressBar = Color.FromArgb(255, 62, 125, 50),
            }.Complete();
        }
    }
}
