using System;
using System.Drawing;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Random theme: a new colour every time it is applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The old version drew every channel of every colour at random, text included,
    /// so a good half of the draws came out unreadable. This version picks one
    /// random hue and one random brightness, and derives every surface, text and
    /// accent from that hue.
    /// </para>
    /// <para>
    /// The result is still unpredictable, which is the point of the theme, but the
    /// colours can no longer clash and the text is always readable on the surface it
    /// sits on.
    /// </para>
    /// </remarks>
    internal class CrazyTheme : IUITheme
    {
        public string Name { get { return "Crazy"; } }

        private readonly ThemePalette _palette = BuildPalette();

        public ThemePalette Palette { get { return _palette; } }

        private static ThemePalette BuildPalette()
        {
            var random = new Random();
            var hue = random.Next(0, 360);

            return random.Next(0, 2) == 0 ? BuildDarkPalette(hue) : BuildLightPalette(hue);
        }

        private static ThemePalette BuildDarkPalette(double hue)
        {
            var accent = ThemePalette.Hsl(hue, 0.62, 0.58);

            return new ThemePalette
            {
                IsDark = true,

                Window = ThemePalette.Hsl(hue, 0.22, 0.11),
                Surface = ThemePalette.Hsl(hue, 0.20, 0.16),
                SurfaceAlt = ThemePalette.Hsl(hue, 0.18, 0.22),
                Border = ThemePalette.Hsl(hue, 0.16, 0.32),

                Text = ThemePalette.Hsl(hue, 0.10, 0.95),
                TextMuted = ThemePalette.Hsl(hue, 0.12, 0.68),

                TitleBar = ThemePalette.Hsl(hue, 0.30, 0.24),

                Accent = accent,

                ButtonFace = ThemePalette.Hsl(hue, 0.18, 0.26),
                ButtonBorder = ThemePalette.Hsl(hue, 0.16, 0.34),

                InputFace = ThemePalette.Hsl(hue, 0.22, 0.13),
                InputBorder = ThemePalette.Hsl(hue, 0.16, 0.34),

                TabFace = ThemePalette.Hsl(hue, 0.22, 0.13),
                TabSelectedFace = ThemePalette.Hsl(hue, 0.20, 0.20),

                Link = ThemePalette.Hsl(hue, 0.60, 0.72),
                Selection = ThemePalette.Hsl(hue, 0.45, 0.38),

                ProgressBar = accent,
            }.Complete();
        }

        private static ThemePalette BuildLightPalette(double hue)
        {
            var accent = ThemePalette.Hsl(hue, 0.62, 0.42);

            return new ThemePalette
            {
                IsDark = false,

                Window = ThemePalette.Hsl(hue, 0.40, 0.93),
                Surface = ThemePalette.Hsl(hue, 0.35, 0.98),
                SurfaceAlt = ThemePalette.Hsl(hue, 0.38, 0.94),
                Border = ThemePalette.Hsl(hue, 0.25, 0.78),

                Text = ThemePalette.Hsl(hue, 0.35, 0.14),
                TextMuted = ThemePalette.Hsl(hue, 0.20, 0.42),

                TitleBar = ThemePalette.Hsl(hue, 0.45, 0.30),
                TitleBarText = ThemePalette.Hsl(hue, 0.30, 0.97),

                Accent = accent,

                ButtonFace = ThemePalette.Hsl(hue, 0.38, 0.95),
                ButtonBorder = ThemePalette.Hsl(hue, 0.25, 0.80),

                InputFace = ThemePalette.Hsl(hue, 0.20, 1.0),
                InputBorder = ThemePalette.Hsl(hue, 0.25, 0.80),

                TabFace = ThemePalette.Hsl(hue, 0.38, 0.90),
                TabSelectedFace = ThemePalette.Hsl(hue, 0.35, 0.98),

                Link = ThemePalette.Hsl(hue, 0.70, 0.36),
                Selection = ThemePalette.Hsl(hue, 0.55, 0.78),

                ProgressBar = accent,
            }.Complete();
        }
    }
}
