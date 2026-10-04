using System.Drawing;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.ui.themes.variants
{
    /// <summary>
    /// Classic grey theme, the colours Windows used before themes existed: a light
    /// grey face, black text and a navy title bar.
    /// </summary>
    /// <remarks>
    /// The old version of this theme painted the panels grey but left the links and
    /// the drop downs on their designer colours, which made them unreadable. Every
    /// role is now filled in, so the grey look keeps working on the controls the
    /// palette covers.
    /// </remarks>
    internal class OldTheme : IUITheme
    {
        public string Name { get { return "Old"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            return new ThemePalette
            {
                IsDark = false,

                Window = Color.FromArgb(255, 212, 208, 200),
                Surface = Color.FromArgb(255, 212, 208, 200),
                SurfaceAlt = Color.FromArgb(255, 228, 225, 218),
                Border = Color.FromArgb(255, 128, 128, 128),

                Text = Color.FromArgb(255, 0, 0, 0),
                TextMuted = Color.FromArgb(255, 77, 77, 77),

                TitleBar = Color.FromArgb(255, 10, 36, 106),
                TitleBarText = Color.FromArgb(255, 255, 255, 255),
                TitleBarMuted = Color.FromArgb(255, 176, 190, 224),

                Accent = Color.FromArgb(255, 10, 36, 106),
                AccentText = Color.FromArgb(255, 255, 255, 255),

                ButtonFace = Color.FromArgb(255, 212, 208, 200),
                ButtonText = Color.FromArgb(255, 0, 0, 0),
                ButtonHover = Color.FromArgb(255, 226, 224, 218),
                ButtonPressed = Color.FromArgb(255, 196, 192, 184),
                ButtonBorder = Color.FromArgb(255, 128, 128, 128),

                InputFace = Color.FromArgb(255, 255, 255, 255),
                InputText = Color.FromArgb(255, 0, 0, 0),
                InputBorder = Color.FromArgb(255, 128, 128, 128),

                TabFace = Color.FromArgb(255, 212, 208, 200),
                TabSelectedFace = Color.FromArgb(255, 212, 208, 200),
                TabText = Color.FromArgb(255, 60, 60, 60),
                TabSelectedText = Color.FromArgb(255, 0, 0, 0),

                Link = Color.FromArgb(255, 0, 0, 192),
                Selection = Color.FromArgb(255, 10, 36, 106),
                SelectionText = Color.FromArgb(255, 255, 255, 255),

                ProgressBar = Color.FromArgb(255, 10, 36, 106),
            }.Complete();
        }
    }
}
