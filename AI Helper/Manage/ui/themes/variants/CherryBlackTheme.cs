using System.Drawing;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Cherry and black theme: warm near black surfaces with a cherry red accent.
    /// </summary>
    /// <remarks>
    /// The old version of this theme filled every panel with bright cherry red,
    /// which left the labels and the links fighting the background. The red is now
    /// the accent and the title bar, and the surfaces it used to flood are dark, so
    /// the theme keeps its identity and stays readable.
    /// </remarks>
    internal class CherryBlackTheme : IUITheme
    {
        public string Name { get { return "CherryBlack"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            return new ThemePalette
            {
                IsDark = true,

                Window = Color.FromArgb(255, 26, 22, 20),
                Surface = Color.FromArgb(255, 45, 41, 38),
                SurfaceAlt = Color.FromArgb(255, 58, 52, 49),
                Border = Color.FromArgb(255, 82, 74, 70),

                Text = Color.FromArgb(255, 245, 241, 238),
                TextMuted = Color.FromArgb(255, 176, 164, 157),

                TitleBar = Color.FromArgb(255, 183, 51, 42),
                TitleBarText = Color.FromArgb(255, 255, 244, 242),
                TitleBarMuted = Color.FromArgb(255, 244, 198, 193),

                Accent = Color.FromArgb(255, 233, 76, 61),
                AccentText = Color.FromArgb(255, 255, 255, 255),

                ButtonFace = Color.FromArgb(255, 58, 52, 49),
                ButtonText = Color.FromArgb(255, 245, 241, 238),
                ButtonHover = Color.FromArgb(255, 74, 66, 62),
                ButtonPressed = Color.FromArgb(255, 48, 42, 39),
                ButtonBorder = Color.FromArgb(255, 82, 74, 70),

                Danger = Color.FromArgb(255, 233, 76, 61),
                DangerText = Color.FromArgb(255, 255, 255, 255),

                InputFace = Color.FromArgb(255, 36, 31, 29),
                InputText = Color.FromArgb(255, 245, 241, 238),
                InputBorder = Color.FromArgb(255, 82, 74, 70),

                TabFace = Color.FromArgb(255, 36, 31, 29),
                TabSelectedFace = Color.FromArgb(255, 45, 41, 38),
                TabText = Color.FromArgb(255, 176, 164, 157),
                TabSelectedText = Color.FromArgb(255, 255, 255, 255),

                Link = Color.FromArgb(255, 255, 138, 122),
                Selection = Color.FromArgb(255, 122, 42, 34),
                SelectionText = Color.FromArgb(255, 255, 255, 255),

                ProgressBar = Color.FromArgb(255, 233, 76, 61),
            }.Complete();
        }
    }
}
