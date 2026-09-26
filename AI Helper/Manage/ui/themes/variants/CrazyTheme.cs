using System;
using System.Drawing;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Random theme: a new set of colours every time it is applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every role of the palette is drawn separately, from its own hue, saturation and
    /// lightness. The window is therefore genuinely a different set of colours on every
    /// draw rather than one hue shaded a few ways, which is what it used to be and what
    /// made it look the same every time.
    /// </para>
    /// <para>
    /// Each role is still drawn inside a range it has to stay in to be readable: a
    /// surface is always dark on a dark theme, text is always light on it, and a border
    /// is always something that can be told apart from the surface it outlines. Random
    /// is the point of the theme, unreadable is not, and the old version - which drew
    /// every channel of every colour, text included - was unreadable about half the
    /// time.
    /// </para>
    /// <para>
    /// One role is not free to be any colour at all: red is what makes a close button
    /// and a destructive action read as what they are, so the danger colour stays in
    /// the warm end of the wheel. Everything else is free.
    /// </para>
    /// </remarks>
    internal class CrazyTheme : IUITheme, IRollableTheme
    {
        public string Name { get { return "Crazy"; } }

        private ThemePalette _palette;

        /// <summary>
        /// The colours drawn by the last <see cref="Roll"/>.
        /// </summary>
        /// <remarks>
        /// Drawn on first use rather than in the constructor, because the theme list
        /// builds an instance of every theme just to read their names, and only the one
        /// which is actually applied needs colours.
        /// </remarks>
        public ThemePalette Palette
        {
            get
            {
                if (_palette == null) Roll();

                return _palette;
            }
        }

        /// <summary>
        /// Draws a new set of colours, replacing the ones drawn before.
        /// </summary>
        public void Roll()
        {
            _palette = BuildPalette();
        }

        /// <summary>
        /// The one generator every draw goes through.
        /// </summary>
        /// <remarks>
        /// A generator of its own per draw would not be random at all: <c>new
        /// Random()</c> is seeded from the clock, so two themes built in the same
        /// millisecond - which is exactly what filling the theme list does - would come
        /// out with the very same colours. That is why the old version repeated itself.
        /// One shared generator does not have that problem. It is locked because the
        /// list can be built from another thread.
        /// </remarks>
        private static readonly Random Generator = new Random();

        private static int NextRandom(int minValue, int maxValue)
        {
            lock (Generator)
            {
                return Generator.Next(minValue, maxValue);
            }
        }

        /// <summary>
        /// A random value in the given range, used to give one role a shade of its own
        /// inside the range it has to stay in.
        /// </summary>
        private static double Jitter(double min, double max)
        {
            return min + (NextRandom(0, 1024) / 1024d) * (max - min);
        }

        private static ThemePalette BuildPalette()
        {
            var isDark = NextRandom(0, 2) == 0;

            // three independent hues: the surfaces, the accent and the text each get
            // their own, so the colours can differ from each other instead of all being
            // one hue at three lightnesses
            var surfaceHue = NextRandom(0, 360);
            var accentHue = NextRandom(0, 360);
            var textHue = NextRandom(0, 360);

            // the warm end of the wheel, either side of red
            var dangerHue = NextRandom(345, 375) % 360;

            return isDark
                ? BuildDarkPalette(surfaceHue, accentHue, textHue, dangerHue)
                : BuildLightPalette(surfaceHue, accentHue, textHue, dangerHue);
        }

        private static ThemePalette BuildDarkPalette(double surfaceHue, double accentHue, double textHue, double dangerHue)
        {
            var accent = ThemePalette.Hsl(accentHue, Jitter(0.52, 0.72), Jitter(0.52, 0.64));
            var accentHover = ThemePalette.Hsl(accentHue, Jitter(0.52, 0.72), Jitter(0.64, 0.74));
            var danger = ThemePalette.Hsl(dangerHue, Jitter(0.52, 0.72), Jitter(0.48, 0.60));
            var selection = ThemePalette.Hsl(accentHue, Jitter(0.30, 0.55), Jitter(0.32, 0.44));

            return new ThemePalette
            {
                IsDark = true,

                // surfaces: dark, and only tinted enough to be recognisably coloured
                Window = ThemePalette.Hsl(surfaceHue, Jitter(0.14, 0.28), Jitter(0.07, 0.12)),
                Surface = ThemePalette.Hsl(surfaceHue, Jitter(0.12, 0.26), Jitter(0.13, 0.18)),
                SurfaceAlt = ThemePalette.Hsl(surfaceHue, Jitter(0.10, 0.24), Jitter(0.19, 0.25)),
                Border = ThemePalette.Hsl(surfaceHue, Jitter(0.08, 0.22), Jitter(0.28, 0.38)),

                // text: light, and tinted so it belongs to the theme
                Text = ThemePalette.Hsl(textHue, Jitter(0.03, 0.16), Jitter(0.91, 0.98)),
                TextMuted = ThemePalette.Hsl(textHue, Jitter(0.05, 0.20), Jitter(0.60, 0.72)),

                TitleBar = ThemePalette.Hsl(surfaceHue, Jitter(0.20, 0.36), Jitter(0.21, 0.29)),
                TitleBarText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.16), Jitter(0.91, 0.98)),
                TitleBarMuted = ThemePalette.Hsl(textHue, Jitter(0.05, 0.20), Jitter(0.58, 0.70)),

                Accent = accent,
                AccentText = ThemePalette.Contrast(accent),
                AccentHover = accentHover,

                Danger = danger,
                DangerText = ThemePalette.Contrast(danger),
                DangerHover = ThemePalette.Hsl(dangerHue, Jitter(0.52, 0.72), Jitter(0.58, 0.68)),

                ButtonFace = ThemePalette.Hsl(surfaceHue, Jitter(0.12, 0.26), Jitter(0.23, 0.29)),
                ButtonText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.16), Jitter(0.89, 0.97)),
                ButtonHover = ThemePalette.Hsl(surfaceHue, Jitter(0.14, 0.30), Jitter(0.29, 0.37)),
                ButtonPressed = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.32), Jitter(0.35, 0.43)),
                ButtonBorder = ThemePalette.Hsl(surfaceHue, Jitter(0.08, 0.22), Jitter(0.30, 0.40)),
                ButtonDisabledText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.14), Jitter(0.42, 0.52)),

                // inputs sit lower than the surface around them, as if they were cut into it
                InputFace = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.30), Jitter(0.09, 0.14)),
                InputText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.16), Jitter(0.89, 0.97)),
                InputBorder = ThemePalette.Hsl(surfaceHue, Jitter(0.08, 0.22), Jitter(0.30, 0.40)),
                InputDisabledText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.14), Jitter(0.42, 0.52)),

                TabFace = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.30), Jitter(0.09, 0.14)),
                TabSelectedFace = ThemePalette.Hsl(surfaceHue, Jitter(0.14, 0.28), Jitter(0.17, 0.23)),
                TabText = ThemePalette.Hsl(textHue, Jitter(0.05, 0.20), Jitter(0.60, 0.72)),
                TabSelectedText = ThemePalette.Hsl(textHue, Jitter(0.03, 0.16), Jitter(0.91, 0.98)),

                Link = ThemePalette.Hsl(accentHue, Jitter(0.44, 0.68), Jitter(0.64, 0.78)),
                LinkHover = ThemePalette.Hsl(accentHue, Jitter(0.44, 0.68), Jitter(0.78, 0.90)),
                LinkVisited = ThemePalette.Hsl(accentHue, Jitter(0.36, 0.60), Jitter(0.56, 0.70)),

                Selection = selection,
                SelectionText = ThemePalette.Contrast(selection),

                ProgressBar = accent,
                ProgressBarTrack = ThemePalette.Hsl(surfaceHue, Jitter(0.10, 0.24), Jitter(0.15, 0.21)),
            }.Complete();
        }

        private static ThemePalette BuildLightPalette(double surfaceHue, double accentHue, double textHue, double dangerHue)
        {
            var accent = ThemePalette.Hsl(accentHue, Jitter(0.52, 0.74), Jitter(0.36, 0.46));
            var accentHover = ThemePalette.Hsl(accentHue, Jitter(0.52, 0.74), Jitter(0.28, 0.38));
            var danger = ThemePalette.Hsl(dangerHue, Jitter(0.55, 0.75), Jitter(0.38, 0.48));
            var selection = ThemePalette.Hsl(accentHue, Jitter(0.40, 0.62), Jitter(0.74, 0.84));

            return new ThemePalette
            {
                IsDark = false,

                Window = ThemePalette.Hsl(surfaceHue, Jitter(0.30, 0.50), Jitter(0.90, 0.94)),
                Surface = ThemePalette.Hsl(surfaceHue, Jitter(0.24, 0.46), Jitter(0.95, 0.99)),
                SurfaceAlt = ThemePalette.Hsl(surfaceHue, Jitter(0.28, 0.48), Jitter(0.91, 0.95)),
                Border = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.34), Jitter(0.74, 0.83)),

                Text = ThemePalette.Hsl(textHue, Jitter(0.20, 0.40), Jitter(0.10, 0.17)),
                TextMuted = ThemePalette.Hsl(textHue, Jitter(0.14, 0.30), Jitter(0.36, 0.46)),

                TitleBar = ThemePalette.Hsl(surfaceHue, Jitter(0.34, 0.52), Jitter(0.26, 0.34)),
                TitleBarText = ThemePalette.Hsl(textHue, Jitter(0.04, 0.18), Jitter(0.93, 0.99)),
                TitleBarMuted = ThemePalette.Hsl(textHue, Jitter(0.06, 0.20), Jitter(0.74, 0.84)),

                Accent = accent,
                AccentText = ThemePalette.Contrast(accent),
                AccentHover = accentHover,

                Danger = danger,
                DangerText = ThemePalette.Contrast(danger),
                DangerHover = ThemePalette.Hsl(dangerHue, Jitter(0.55, 0.75), Jitter(0.30, 0.40)),

                ButtonFace = ThemePalette.Hsl(surfaceHue, Jitter(0.28, 0.48), Jitter(0.92, 0.96)),
                ButtonText = ThemePalette.Hsl(textHue, Jitter(0.20, 0.40), Jitter(0.10, 0.18)),
                ButtonHover = ThemePalette.Hsl(surfaceHue, Jitter(0.30, 0.52), Jitter(0.87, 0.92)),
                ButtonPressed = ThemePalette.Hsl(surfaceHue, Jitter(0.32, 0.54), Jitter(0.82, 0.88)),
                ButtonBorder = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.34), Jitter(0.72, 0.82)),
                ButtonDisabledText = ThemePalette.Hsl(textHue, Jitter(0.10, 0.24), Jitter(0.58, 0.68)),

                InputFace = ThemePalette.Hsl(surfaceHue, Jitter(0.14, 0.30), Jitter(0.97, 1.0)),
                InputText = ThemePalette.Hsl(textHue, Jitter(0.20, 0.40), Jitter(0.10, 0.18)),
                InputBorder = ThemePalette.Hsl(surfaceHue, Jitter(0.16, 0.34), Jitter(0.72, 0.82)),
                InputDisabledText = ThemePalette.Hsl(textHue, Jitter(0.10, 0.24), Jitter(0.58, 0.68)),

                TabFace = ThemePalette.Hsl(surfaceHue, Jitter(0.30, 0.50), Jitter(0.86, 0.92)),
                TabSelectedFace = ThemePalette.Hsl(surfaceHue, Jitter(0.26, 0.46), Jitter(0.95, 0.99)),
                TabText = ThemePalette.Hsl(textHue, Jitter(0.14, 0.30), Jitter(0.36, 0.46)),
                TabSelectedText = ThemePalette.Hsl(textHue, Jitter(0.20, 0.40), Jitter(0.10, 0.17)),

                Link = ThemePalette.Hsl(accentHue, Jitter(0.58, 0.80), Jitter(0.30, 0.40)),
                LinkHover = ThemePalette.Hsl(accentHue, Jitter(0.58, 0.80), Jitter(0.22, 0.32)),
                LinkVisited = ThemePalette.Hsl(accentHue, Jitter(0.40, 0.62), Jitter(0.38, 0.48)),

                Selection = selection,
                SelectionText = ThemePalette.Contrast(selection),

                ProgressBar = accent,
                ProgressBarTrack = ThemePalette.Hsl(surfaceHue, Jitter(0.20, 0.40), Jitter(0.84, 0.90)),
            }.Complete();
        }
    }
}
