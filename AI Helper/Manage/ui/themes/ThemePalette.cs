using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// The colours of one theme, named after the role a control plays instead of
    /// after the control itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The old model listed one entry per control type, so every control the list
    /// did not mention (a <see cref="System.Windows.Forms.ComboBox"/>, a link, a
    /// progress bar) kept its designer colour and the theme came out half applied.
    /// Naming the colours after their role fixes that: a theme only has to say what
    /// "the accent colour" or "a text box face" is, and the loader knows which
    /// control asks for which role.
    /// </para>
    /// <para>
    /// Every theme has to fill in the whole palette. Leaving a role empty would put
    /// the control back on its designer colour, which is exactly the bug this
    /// replaces, so <see cref="Validate"/> reports anything left unset.
    /// </para>
    /// </remarks>
    public class ThemePalette
    {
        /// <summary>
        /// Every colour role of a palette, in the order they are written and shown.
        /// </summary>
        /// <remarks>
        /// Discovered rather than listed, and the one place the set of roles is decided:
        /// <see cref="Validate"/> walks it, the theme store writes and reads it, and the
        /// theme editor offers it. A role added to this class is therefore validated,
        /// saved, read back and made editable without anything else being touched.
        /// <para>
        /// The order is by name so that a saved file, the editor's list and a comparison
        /// between two palettes all run through the roles the same way, and so that adding
        /// a role appends it to a file instead of reshuffling every line.
        /// </para>
        /// </remarks>
        private static readonly PropertyInfo[] ColourRoleList = typeof(ThemePalette)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite)
            .Where(property => property.PropertyType == typeof(Color))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        /// <summary>
        /// Colour of the window itself, behind the content surfaces.
        /// </summary>
        public Color Window { get; set; }

        /// <summary>
        /// Colour of the content surfaces: panels, tab pages and the like.
        /// </summary>
        public Color Surface { get; set; }

        /// <summary>
        /// Colour of a surface which has to stand out from <see cref="Surface"/>,
        /// such as a group box or a nested panel.
        /// </summary>
        public Color SurfaceAlt { get; set; }

        /// <summary>
        /// Colour of the thin line between surfaces, and of the border a borderless
        /// window draws for itself.
        /// </summary>
        public Color Border { get; set; }

        /// <summary>
        /// Colour of normal text.
        /// </summary>
        public Color Text { get; set; }

        /// <summary>
        /// Colour of secondary text: hints, captions, disabled labels.
        /// </summary>
        public Color TextMuted { get; set; }

        /// <summary>
        /// Colour of the custom title bar.
        /// </summary>
        public Color TitleBar { get; set; }

        /// <summary>
        /// Colour of text drawn on the title bar.
        /// </summary>
        public Color TitleBarText { get; set; }

        /// <summary>
        /// Colour of secondary text drawn on the title bar.
        /// </summary>
        public Color TitleBarMuted { get; set; }

        /// <summary>
        /// Colour which marks the primary action or the active element.
        /// </summary>
        public Color Accent { get; set; }

        /// <summary>
        /// Colour of text drawn on <see cref="Accent"/>.
        /// </summary>
        public Color AccentText { get; set; }

        /// <summary>
        /// Colour of the accent when the pointer is over it.
        /// </summary>
        public Color AccentHover { get; set; }

        /// <summary>
        /// Face colour of a button at rest.
        /// </summary>
        public Color ButtonFace { get; set; }

        /// <summary>
        /// Colour of the text on a button.
        /// </summary>
        public Color ButtonText { get; set; }

        /// <summary>
        /// Face colour of a button under the pointer.
        /// </summary>
        public Color ButtonHover { get; set; }

        /// <summary>
        /// Face colour of a button while it is pressed.
        /// </summary>
        public Color ButtonPressed { get; set; }

        /// <summary>
        /// Colour of a button outline.
        /// </summary>
        public Color ButtonBorder { get; set; }

        /// <summary>
        /// Colour of the text of a disabled button.
        /// </summary>
        public Color ButtonDisabledText { get; set; }

        /// <summary>
        /// Face colour of a button which destroys something, such as close.
        /// </summary>
        public Color Danger { get; set; }

        /// <summary>
        /// Face colour of a destructive button under the pointer.
        /// </summary>
        public Color DangerHover { get; set; }

        /// <summary>
        /// Colour of text drawn on <see cref="Danger"/>.
        /// </summary>
        public Color DangerText { get; set; }

        /// <summary>
        /// Face colour of an editable control: text box, drop down, list.
        /// </summary>
        public Color InputFace { get; set; }

        /// <summary>
        /// Colour of the text inside an editable control.
        /// </summary>
        public Color InputText { get; set; }

        /// <summary>
        /// Colour of an editable control outline.
        /// </summary>
        public Color InputBorder { get; set; }

        /// <summary>
        /// Colour of the text of a read only or disabled editable control.
        /// </summary>
        public Color InputDisabledText { get; set; }

        /// <summary>
        /// Face colour of the tab strip, behind the tab headers.
        /// </summary>
        public Color TabFace { get; set; }

        /// <summary>
        /// Face colour of the selected tab header.
        /// </summary>
        public Color TabSelectedFace { get; set; }

        /// <summary>
        /// Colour of the text of an unselected tab header.
        /// </summary>
        public Color TabText { get; set; }

        /// <summary>
        /// Colour of the text of the selected tab header.
        /// </summary>
        public Color TabSelectedText { get; set; }

        /// <summary>
        /// Colour of the outline drawn around a tab header.
        /// </summary>
        /// <remarks>
        /// A tab strip draws a two pixel frame around every header itself, in a
        /// system colour no property reaches, so the frame is drawn by the theme
        /// instead and this is what it is drawn in. A theme which leaves it out gets
        /// the ordinary border colour, which is what the frame around an unselected
        /// header used to be drawn with.
        /// </remarks>
        public Color TabBorder { get; set; }

        /// <summary>
        /// Face colour of a group box: the panel its contents sit on.
        /// </summary>
        public Color GroupFace { get; set; }

        /// <summary>
        /// Colour of the caption of a group box.
        /// </summary>
        public Color GroupText { get; set; }

        /// <summary>
        /// Colour of the frame around a group box.
        /// </summary>
        public Color GroupBorder { get; set; }

        /// <summary>
        /// Face colour of the box of a check box, in every state.
        /// </summary>
        public Color CheckFace { get; set; }

        /// <summary>
        /// Colour of the caption of a check box.
        /// </summary>
        public Color CheckText { get; set; }

        /// <summary>
        /// Colour of the outline of the box of a check box.
        /// </summary>
        public Color CheckBorder { get; set; }

        /// <summary>
        /// Colour of the tick inside a checked box.
        /// </summary>
        public Color CheckMark { get; set; }

        /// <summary>
        /// Colour of the application name on the launch page.
        /// </summary>
        /// <remarks>
        /// Named apart from <see cref="Accent"/> on purpose: the name is the largest
        /// text in the window, and a theme which wants a loud accent on its buttons
        /// usually does not want the same colour behind its name.
        /// </remarks>
        public Color LaunchTitleText { get; set; }

        /// <summary>
        /// Colour of the name of the selected game, under the application name.
        /// </summary>
        public Color LaunchGameText { get; set; }

        /// <summary>
        /// Colour of a link at rest.
        /// </summary>
        public Color Link { get; set; }

        /// <summary>
        /// Colour of a link which has already been followed.
        /// </summary>
        public Color LinkVisited { get; set; }

        /// <summary>
        /// Colour of the text of a link under the pointer.
        /// </summary>
        public Color LinkHover { get; set; }

        /// <summary>
        /// Background of a selected row in a list.
        /// </summary>
        public Color Selection { get; set; }

        /// <summary>
        /// Colour of the text of a selected row in a list.
        /// </summary>
        public Color SelectionText { get; set; }

        /// <summary>
        /// Colour of the filled part of a progress bar.
        /// </summary>
        public Color ProgressBar { get; set; }

        /// <summary>
        /// Colour of the empty part of a progress bar.
        /// </summary>
        public Color ProgressBarTrack { get; set; }

        /// <summary>
        /// Tells whether the theme draws light text on dark surfaces. Used where a
        /// colour has to be derived instead of named.
        /// </summary>
        public bool IsDark { get; set; }

        /// <summary>
        /// Fills in every role a theme left out by deriving it from the ones it did
        /// set, and returns the same instance so a theme can write
        /// <c>return new ThemePalette { ... }.Complete();</c>.
        /// </summary>
        /// <remarks>
        /// A theme should only have to state what makes it recognisable: its
        /// surfaces, its text and its accent. Hover shades, borders and disabled
        /// text follow from those, and deriving them here means every theme gets
        /// them consistently instead of each one inventing its own.
        /// </remarks>
        public ThemePalette Complete()
        {
            var contrastDirection = IsDark ? Color.White : Color.Black;

            // The accent is the one role that several others are derived from and that
            // nothing derived, so it is settled first. A palette which does not say what
            // its accent is gets a neutral one taken from its surface, rather than leaving
            // every accent coloured control on its designer colour.
            if (Accent.IsEmpty && !Surface.IsEmpty) Accent = Blend(Surface, Contrast(Surface), 0.55f);

            // roles a theme can leave out entirely, defaulted before anything is
            // derived from them
            if (Link.IsEmpty) Link = Accent;
            if (Danger.IsEmpty) Danger = Color.FromArgb(255, 196, 43, 28);
            if (Selection.IsEmpty) Selection = Blend(Accent, Surface, 0.65f);
            if (ProgressBar.IsEmpty) ProgressBar = Accent;

            if (Window.IsEmpty) Window = Surface;

            // Text is settled before the roles which are derived from it, and it is derived
            // from the surface it is read on. A theme file which leaves it out, or spells it
            // so badly that it cannot be read, would otherwise come back with every label on
            // the designer colour it was given - which is the failure this method exists to
            // prevent.
            if (Text.IsEmpty) Text = Contrast(Surface);

            if (SurfaceAlt.IsEmpty) SurfaceAlt = Blend(Surface, contrastDirection, 0.04f);
            if (Border.IsEmpty) Border = Blend(Surface, Text, 0.18f);
            if (TextMuted.IsEmpty) TextMuted = Blend(Text, Surface, 0.42f);

            if (TitleBar.IsEmpty) TitleBar = SurfaceAlt;
            if (TitleBarText.IsEmpty) TitleBarText = Contrast(TitleBar);
            if (TitleBarMuted.IsEmpty) TitleBarMuted = Blend(TitleBarText, TitleBar, 0.38f);

            if (AccentText.IsEmpty) AccentText = Contrast(Accent);
            if (AccentHover.IsEmpty) AccentHover = Blend(Accent, contrastDirection, 0.15f);

            if (ButtonFace.IsEmpty) ButtonFace = SurfaceAlt;
            if (ButtonText.IsEmpty) ButtonText = Contrast(ButtonFace);
            if (ButtonHover.IsEmpty) ButtonHover = IsDark ? Blend(ButtonFace, Color.White, 0.10f) : Blend(ButtonFace, Accent, 0.14f);
            if (ButtonPressed.IsEmpty) ButtonPressed = IsDark ? Blend(ButtonFace, Color.White, 0.18f) : Blend(ButtonFace, Accent, 0.26f);
            if (ButtonBorder.IsEmpty) ButtonBorder = Border;
            if (ButtonDisabledText.IsEmpty) ButtonDisabledText = Blend(ButtonText, ButtonFace, 0.55f);

            if (DangerText.IsEmpty) DangerText = Contrast(Danger);
            if (DangerHover.IsEmpty) DangerHover = Blend(Danger, contrastDirection, 0.12f);

            if (InputFace.IsEmpty) InputFace = Surface;
            if (InputText.IsEmpty) InputText = Contrast(InputFace);
            if (InputBorder.IsEmpty) InputBorder = Border;
            if (InputDisabledText.IsEmpty) InputDisabledText = Blend(InputText, InputFace, 0.55f);

            if (TabFace.IsEmpty) TabFace = SurfaceAlt;
            if (TabSelectedFace.IsEmpty) TabSelectedFace = Surface;
            if (TabText.IsEmpty) TabText = TextMuted;
            if (TabSelectedText.IsEmpty) TabSelectedText = Text;
            if (TabBorder.IsEmpty) TabBorder = Border;

            // The roles of the controls which are painted by the theme rather than by
            // Windows. Each falls back to the role it borrowed before it existed, so a
            // theme written before them looks the same with them as without.
            if (GroupFace.IsEmpty) GroupFace = SurfaceAlt;
            if (GroupText.IsEmpty) GroupText = Text;
            if (GroupBorder.IsEmpty) GroupBorder = Border;

            if (CheckFace.IsEmpty) CheckFace = InputFace;
            if (CheckText.IsEmpty) CheckText = Text;
            if (CheckBorder.IsEmpty) CheckBorder = Border;

            // the tick is a small shape on the box face, so it is derived the way every
            // other mark on a surface is: the one of black and white which is readable
            // there. A theme which would rather have its accent in the box can say so.
            if (CheckMark.IsEmpty) CheckMark = Contrast(CheckFace);

            if (LaunchTitleText.IsEmpty) LaunchTitleText = Accent;
            if (LaunchGameText.IsEmpty) LaunchGameText = TextMuted;

            if (LinkHover.IsEmpty) LinkHover = Blend(Link, contrastDirection, 0.20f);
            if (LinkVisited.IsEmpty) LinkVisited = Link;
            if (SelectionText.IsEmpty) SelectionText = Contrast(Selection);
            if (ProgressBarTrack.IsEmpty) ProgressBarTrack = Blend(InputFace, Border, 0.6f);

            return this;
        }

        /// <summary>
        /// The colour roles of a palette, in the order they are written and shown.
        /// </summary>
        /// <remarks>
        /// The list behind <see cref="Validate"/>, the theme store and the theme editor, so
        /// all three agree on what a role is without any of them keeping its own copy.
        /// </remarks>
        internal static IReadOnlyList<PropertyInfo> ColourRoles()
        {
            return ColourRoleList;
        }

        /// <summary>
        /// Returns the first role which was never assigned. A palette which reports
        /// a name here is incomplete and will leave that part of the window on its
        /// designer colour.
        /// </summary>
        /// <remarks>
        /// Walks the discovered roles rather than a list written out here, so a role added
        /// to this class is covered without this method being remembered.
        /// </remarks>
        public string Validate()
        {
            foreach (var role in ColourRoleList)
            {
                if (((Color)role.GetValue(this, null)).IsEmpty) return role.Name;
            }

            return null;
        }

        /// <summary>
        /// Blends <paramref name="color"/> towards <paramref name="towards"/> by
        /// <paramref name="amount"/> (0 keeps the colour, 1 returns the target).
        /// </summary>
        /// <remarks>
        /// Used to derive a hover or pressed shade from a face colour, so a theme
        /// does not have to name every shade of every role by hand.
        /// </remarks>
        public static Color Blend(Color color, Color towards, float amount)
        {
            if (amount < 0f) amount = 0f;
            if (amount > 1f) amount = 1f;

            return Color.FromArgb(
                color.A,
                BlendChannel(color.R, towards.R, amount),
                BlendChannel(color.G, towards.G, amount),
                BlendChannel(color.B, towards.B, amount));
        }

        /// <summary>
        /// Returns black or white, whichever is readable on
        /// <paramref name="background"/>.
        /// </summary>
        public static Color Contrast(Color background)
        {
            return Luminance(background) > 0.55 ? Color.Black : Color.White;
        }

        /// <summary>
        /// Picks the colour of <paramref name="a"/> or <paramref name="b"/> which
        /// contrasts better with <paramref name="background"/>.
        /// </summary>
        public static Color Contrast(Color background, Color a, Color b)
        {
            var differenceA = Math.Abs(Luminance(background) - Luminance(a));
            var differenceB = Math.Abs(Luminance(background) - Luminance(b));

            return differenceA >= differenceB ? a : b;
        }

        /// <summary>
        /// Returns a colour as close to <paramref name="color"/> as this theme allows,
        /// so it stays readable on the surfaces of this theme.
        /// </summary>
        /// <remarks>
        /// Some parts of the interface pick their own colour to tell one function from
        /// another. Those colours were chosen for a dark background, so on a light
        /// theme they have to be darkened before they can be read at all. Only the
        /// brightness is adjusted, so the hue - which is the part that carries the
        /// meaning - is kept.
        /// </remarks>
        public Color MakeReadable(Color color)
        {
            var luminance = Luminance(color);

            if (IsDark) return luminance >= 0.5f ? color : Blend(color, Color.White, 0.45f);
            return luminance <= 0.5f ? color : Blend(color, Color.Black, 0.5f);
        }

        /// <summary>
        /// Builds a colour from a hue in degrees, a saturation and a lightness, both
        /// between 0 and 1.
        /// </summary>
        /// <remarks>
        /// Used by the random theme, which picks one hue and derives every surface
        /// from it. Deriving from a single hue is what keeps a randomly coloured
        /// window readable: the colours can not clash, because they all share a hue
        /// and only differ in saturation and lightness.
        /// </remarks>
        public static Color Hsl(double hue, double saturation, double lightness)
        {
            hue = hue % 360;
            if (hue < 0) hue += 360;

            saturation = Clamp(saturation);
            lightness = Clamp(lightness);

            var chroma = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            var secondary = chroma * (1 - Math.Abs(hue / 60 % 2 - 1));
            var offset = lightness - chroma / 2;

            double red, green, blue;

            if (hue < 60) { red = chroma; green = secondary; blue = 0; }
            else if (hue < 120) { red = secondary; green = chroma; blue = 0; }
            else if (hue < 180) { red = 0; green = chroma; blue = secondary; }
            else if (hue < 240) { red = 0; green = secondary; blue = chroma; }
            else if (hue < 300) { red = secondary; green = 0; blue = chroma; }
            else { red = chroma; green = 0; blue = secondary; }

            return Color.FromArgb(
                255,
                ToChannel(red + offset),
                ToChannel(green + offset),
                ToChannel(blue + offset));
        }

        /// <summary>
        /// Perceived brightness of a colour, between 0 (black) and 1 (white).
        /// </summary>
        /// <remarks>
        /// The channel weights are the ones the W3C uses for relative luminance; a
        /// plain average would rate a saturated blue as bright as a yellow and pick
        /// the wrong text colour.
        /// </remarks>
        public static float Luminance(Color color)
        {
            return (0.2126f * color.R + 0.7152f * color.G + 0.0722f * color.B) / 255f;
        }

        private static int BlendChannel(int from, int to, float amount)
        {
            return (int)Math.Round(from + (to - from) * amount);
        }

        private static double Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        private static int ToChannel(double value)
        {
            var channel = (int)Math.Round(value * 255);
            if (channel < 0) return 0;
            if (channel > 255) return 255;
            return channel;
        }
    }

    /// <summary>
    /// The type used everywhere in the interface.
    /// </summary>
    /// <remarks>
    /// Fonts are not part of a theme on purpose. They are what makes the window look
    /// like one program instead of six, so they are shared, and a theme only decides
    /// about colour. Controls inherit <see cref="System.Windows.Forms.Control.Font"/>
    /// from their parent, so setting it on the form is enough for the whole window.
    /// </remarks>
    public static class ThemeFonts
    {
        /// <summary>
        /// Family of every text in the interface.
        /// </summary>
        public const string Family = "Segoe UI";

        /// <summary>
        /// Family used for the log box, where the characters have to line up.
        /// </summary>
        public const string MonoFamily = "Consolas";

        /// <summary>
        /// Size of normal text, and the size the window is designed at.
        /// </summary>
        public const float BodySize = 9F;

        /// <summary>
        /// Size of secondary text: captions, hints, status lines.
        /// </summary>
        public const float SmallSize = 8.25F;

        /// <summary>
        /// Normal text.
        /// </summary>
        public static readonly Font Body = new Font(Family, BodySize, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Emphasised normal text, used for button captions and field labels.
        /// </summary>
        public static readonly Font BodyBold = new Font(Family, BodySize, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>
        /// Secondary text.
        /// </summary>
        public static readonly Font Small = new Font(Family, SmallSize, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Emphasised secondary text.
        /// </summary>
        public static readonly Font SmallBold = new Font(Family, SmallSize, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>
        /// Italic secondary text, used for the selected game under the app title.
        /// </summary>
        public static readonly Font SmallItalic = new Font(Family, SmallSize, FontStyle.Italic, GraphicsUnit.Point);

        /// <summary>
        /// Heading text, used for the app title in the title bar.
        /// </summary>
        public static readonly Font Title = new Font(Family, 11F, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>
        /// Large display text, used for the app name on the launch page.
        /// </summary>
        public static readonly Font Hero = new Font(Family, 20F, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Caption of a primary launch button.
        /// </summary>
        public static readonly Font ButtonLarge = new Font(Family, 11F, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Fixed width text, used for logs.
        /// </summary>
        public static readonly Font Mono = new Font(MonoFamily, SmallSize, FontStyle.Regular, GraphicsUnit.Point);
    }
}
