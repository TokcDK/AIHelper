using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using INIFileMan;
using NLog;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Reads and writes the themes the user made.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One file per theme, in a folder beside the settings file. A single file holding
    /// every theme would have to be rewritten whenever one is added or removed, and an
    /// interrupted write would take all of them with it; one file per theme also means
    /// deleting a theme is deleting a file, and renaming one is changing a line inside
    /// it, so no theme ever has to be renumbered.
    /// </para>
    /// <para>
    /// The folder is deliberately not under <c>RES</c>: that tree is replaced when the
    /// application is updated, and a theme the user made is theirs to keep.
    /// </para>
    /// </remarks>
    internal static class CustomThemeStore
    {
        /// <summary>
        /// Section holding the theme inside its own file. One file holds one theme, so
        /// the name only has to be stable.
        /// </summary>
        private const string SectionName = "Theme";

        private const string NameKey = "Name";

        /// <summary>
        /// Key holding whether the theme is a dark one. The only value in the file which is
        /// not a colour, so it is written and read on its own.
        /// </summary>
        private const string IsDarkKey = "IsDark";

        /// <summary>
        /// Prefix of every file this store writes, so it can find its own files without
        /// mistaking anything else in the folder for a theme.
        /// </summary>
        private const string FilePrefix = "theme_";

        private const string FileExtension = ".ini";

        /// <summary>
        /// Longest name a theme may have. Long enough for a date and a note beside it,
        /// short enough to stay readable in the theme list.
        /// </summary>
        internal const int MaxNameLength = 64;

        /// <summary>
        /// Line written above the name, saying how the colours below it are spelled.
        /// </summary>
        /// <remarks>
        /// The file is meant to be opened and changed by hand, so it says what it expects
        /// rather than leaving that to be worked out from the values. It rides on the name
        /// because that is the first key in the file.
        /// </remarks>
        private const string FormatHint =
            "A colour is written as #AARRGGBB - alpha, red, green and blue as hex. The alpha may be left out.";

        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Every theme the user has saved, in the order they are shown.
        /// </summary>
        /// <remarks>
        /// A file which cannot be read is reported and skipped rather than thrown: one
        /// damaged theme must not stop the application from starting or hide the others.
        /// </remarks>
        internal static List<CustomTheme> LoadAll()
        {
            var themes = new List<CustomTheme>();
            var folder = Folder;

            if (!Directory.Exists(folder)) return themes;

            string[] files;
            try
            {
                files = Directory.GetFiles(folder, FilePrefix + "*" + FileExtension);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to list the custom theme folder. Error:\r\n" + ex);
                return themes;
            }

            foreach (var file in files)
            {
                var theme = Load(file);
                if (theme != null) themes.Add(theme);
            }

            return themes.OrderBy(theme => theme.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>
        /// Writes a palette as a new theme and returns it.
        /// </summary>
        internal static CustomTheme Save(string name, ThemePalette palette)
        {
            if (palette == null) return null;

            var theme = new CustomTheme
            {
                Name = Normalise(name),
                Palette = palette,
                FilePath = BuildFilePath()
            };

            return Write(theme) ? theme : null;
        }

        /// <summary>
        /// Changes the name of a stored theme.
        /// </summary>
        /// <remarks>
        /// Only the name inside the file changes. The file keeps its own name, so
        /// nothing which refers to the theme by file has to be found and updated.
        /// </remarks>
        internal static bool Rename(CustomTheme theme, string newName)
        {
            if (theme == null) return false;

            var previous = theme.Name;
            theme.Name = Normalise(newName);

            if (Write(theme)) return true;

            // the write failed, so the theme must not claim a name it does not have
            theme.Name = previous;
            return false;
        }

        /// <summary>
        /// Removes a stored theme.
        /// </summary>
        internal static bool Delete(CustomTheme theme)
        {
            if (theme == null || string.IsNullOrEmpty(theme.FilePath)) return false;

            try
            {
                if (File.Exists(theme.FilePath)) File.Delete(theme.FilePath);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to delete the custom theme '" + theme.Name + "'. Error:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Tells whether a name is already taken by another stored theme.
        /// </summary>
        /// <remarks>
        /// Names have to be unique: the theme list is the only way to choose one, and the
        /// name is what the settings file remembers, so two themes sharing a name would
        /// make the remembered one ambiguous.
        /// </remarks>
        /// <param name="except">
        /// The theme which is allowed to keep the name, which is the one being renamed.
        /// </param>
        internal static bool IsNameTaken(string name, CustomTheme except)
        {
            var candidate = Normalise(name);
            if (candidate.Length == 0) return true;

            var exceptFile = except == null ? null : except.FilePath;

            return LoadAll().Any(theme =>
                !IsStoredIn(theme, exceptFile)
                && string.Equals(theme.Name, candidate, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Tells whether a theme is the one stored in the given file.
        /// </summary>
        /// <remarks>
        /// Decided by the file rather than by the object, because a theme is read from
        /// disk again every time the list is rebuilt: the same theme is a different object
        /// from one moment to the next, and comparing objects would say that a theme
        /// collides with itself. Both paths come from this store, which builds them with
        /// <see cref="Path.Combine"/> over one folder, so they compare as text.
        /// </remarks>
        private static bool IsStoredIn(CustomTheme theme, string filePath)
        {
            if (theme == null || string.IsNullOrEmpty(filePath)) return false;

            return string.Equals(theme.FilePath, filePath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The folder the themes are kept in.
        /// </summary>
        private static string Folder
        {
            get { return ManageSettings.CustomThemesFolder; }
        }

        /// <summary>
        /// A file name no existing theme is using.
        /// </summary>
        /// <remarks>
        /// Built from the moment it is made rather than from the name, so renaming a
        /// theme never has to move a file, and so two themes saved in the same second
        /// still get a file each.
        /// </remarks>
        private static string BuildFilePath()
        {
            var folder = Folder;
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

            var path = Path.Combine(folder, FilePrefix + stamp + FileExtension);
            var suffix = 1;

            while (File.Exists(path))
            {
                path = Path.Combine(folder, FilePrefix + stamp + "_" + suffix.ToString(CultureInfo.InvariantCulture) + FileExtension);
                suffix++;
            }

            return path;
        }

        /// <summary>
        /// Reads one theme file.
        /// </summary>
        private static CustomTheme Load(string path)
        {
            try
            {
                var ini = ManageIni.GetINIFile(path);

                if (!ini.SectionExists(SectionName))
                {
                    _log.Warn("The custom theme file has no '" + SectionName + "' section and is skipped: " + path);
                    return null;
                }

                var theme = new CustomTheme
                {
                    FilePath = path,
                    Name = Normalise(ini.KeyExists(NameKey, SectionName) ? ini.GetKey(SectionName, NameKey) : string.Empty),
                    Palette = ReadPalette(ini, path)
                };

                if (theme.Name.Length == 0)
                {
                    // a theme with no name could not be told apart from another in the
                    // list, so it is named after its file instead of being dropped
                    theme.Name = Path.GetFileNameWithoutExtension(path);
                }

                return theme;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to read the custom theme file " + path + ". Error:\r\n" + ex);
                return null;
            }
        }

        /// <summary>
        /// Writes a theme out, creating the folder when it is not there yet.
        /// </summary>
        private static bool Write(CustomTheme theme)
        {
            if (theme == null || theme.Palette == null || string.IsNullOrEmpty(theme.FilePath)) return false;

            try
            {
                var folder = Path.GetDirectoryName(theme.FilePath);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var ini = ManageIni.GetINIFile(theme.FilePath);

                ini.SetKey(SectionName, NameKey, theme.Name, FormatHint, false);
                WritePalette(ini, theme.Palette);

                // one write for the whole theme: the last argument of SetKey defers the
                // write, and a theme is more than thirty keys
                ini.WriteFile();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to write the custom theme '" + theme.Name + "'. Error:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Writes the palette out, one role per line.
        /// </summary>
        /// <remarks>
        /// A colour is written as <c>#AARRGGBB</c>, the form used wherever colours are kept
        /// in a file, so a theme of the user's own can be opened and changed by hand without
        /// working out what the program meant by the number it stored. The alpha is kept
        /// because a role may carry one, and dropping it would change the colour rather than
        /// only its spelling.
        /// </remarks>
        private static void WritePalette(INIFile ini, ThemePalette palette)
        {
            foreach (var role in ThemePalette.ColourRoles())
            {
                var colour = (Color)role.GetValue(palette, null);

                // a role which was never set is left out rather than written as transparent
                // black, because the two are not the same on the way back in: the missing
                // one is derived from the rest of the palette, the zero one is not
                if (colour.IsEmpty) continue;

                ini.SetKey(SectionName, role.Name, HexColour(colour), false);
            }

            ini.SetKey(SectionName, IsDarkKey, palette.IsDark.ToString(CultureInfo.InvariantCulture), false);
        }

        /// <summary>
        /// A colour as the hex form everyone recognises.
        /// </summary>
        internal static string HexColour(Color colour)
        {
            return "#" + colour.ToArgb().ToString("X8", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Reads a palette back.
        /// </summary>
        /// <remarks>
        /// A role missing from the file is left unset and then derived by
        /// <see cref="ThemePalette.Complete"/>, so a theme written by an older version
        /// still loads and gains the roles which have been added since. A role whose value
        /// cannot be read is reported and skipped rather than thrown, because one damaged
        /// line must not hide the rest of the theme.
        /// </remarks>
        private static ThemePalette ReadPalette(INIFile ini, string path)
        {
            var palette = new ThemePalette();

            foreach (var role in ThemePalette.ColourRoles())
            {
                if (!ini.KeyExists(role.Name, SectionName)) continue;

                var text = ini.GetKey(SectionName, role.Name);

                if (!TryParseColour(text, out var colour))
                {
                    _log.Warn("Role '" + role.Name + "' of the theme file holds '" + text
                        + "', which is not a colour, and is left out: " + path);
                    continue;
                }

                role.SetValue(palette, colour, null);
            }

            if (ini.KeyExists(IsDarkKey, SectionName))
            {
                var text = ini.GetKey(SectionName, IsDarkKey);

                if (TryParseFlag(text, out var isDark))
                {
                    palette.IsDark = isDark;
                }
                else
                {
                    _log.Warn("'" + IsDarkKey + "' of the theme file holds '" + text
                        + "', which is not true or false, and is left out: " + path);
                }
            }

            return palette.Complete();
        }

        /// <summary>
        /// Reads a colour written in any of the spellings a person, or an older version of
        /// this program, is likely to have used.
        /// </summary>
        /// <remarks>
        /// Deliberately forgiving. The file is there to be edited by hand, so a value typed
        /// as <c>#FF0078D7</c>, or left over from the version which wrote the packed number,
        /// has to keep its role. Refusing it would put that part of the window back on a
        /// derived colour without anything saying so.
        /// </remarks>
        internal static bool TryParseColour(string text, out Color colour)
        {
            colour = Color.Empty;

            text = StripComment(text);
            if (text.Length == 0) return false;

            // #AARRGGBB, or #RRGGBB for a colour with no transparency in it
            if (text[0] == '#') return TryParseHex(text.Substring(1), out colour);

            // A, R, G, B
            var parts = text.Split(',');
            if (parts.Length == 4)
            {
                var channels = new int[4];

                for (var i = 0; i < 4; i++)
                {
                    if (!TryParseChannel(parts[i], out channels[i])) return false;
                }

                colour = Color.FromArgb(channels[0], channels[1], channels[2], channels[3]);
                return true;
            }

            // a name from the known colour table, such as Red or ControlText
            var named = Color.FromName(text);
            if (named.IsKnownColor)
            {
                // taken apart and put back together, so what is handed out is the colour
                // itself rather than one which also claims to be that name
                colour = Color.FromArgb(named.A, named.R, named.G, named.B);
                return true;
            }

            // the packed number an older version of this program wrote
            if (TryParseArgb(text, out var argb))
            {
                colour = Color.FromArgb(argb);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a comment a person left on the same line as the value, and the quotes
        /// they may have wrapped it in.
        /// </summary>
        /// <remarks>
        /// The ini library only takes off the comments it wrote itself, which sit on their
        /// own line; one typed after a value is handed back as part of it, and so is a pair
        /// of quotes. A <c>#</c> starts a comment only once the value has begun, because a
        /// value which starts with one is the hex form - and the quotes come off first, or
        /// the <c>#</c> inside them would look like the start of a comment.
        /// </remarks>
        private static string StripComment(string text)
        {
            if (text == null) return string.Empty;

            text = TrimQuotes(text.Trim());

            var semicolon = text.IndexOf(';');
            if (semicolon >= 0) text = text.Substring(0, semicolon);

            // the quotes may have been on the other side of the note, and the hash only
            // means a comment once they are off
            text = TrimQuotes(text.Trim());

            var hash = text.IndexOf('#', text.Length > 0 && text[0] == '#' ? 1 : 0);
            if (hash >= 0) text = text.Substring(0, hash);

            return TrimQuotes(text.Trim());
        }

        /// <summary>
        /// Takes a pair of quotes off a value, if it is wrapped in one.
        /// </summary>
        private static string TrimQuotes(string text)
        {
            if (text.Length < 2) return text;
            if (text[0] != '"' || text[text.Length - 1] != '"') return text;

            return text.Substring(1, text.Length - 2).Trim();
        }

        /// <summary>
        /// Reads the digits of a hex colour, with or without the alpha.
        /// </summary>
        private static bool TryParseHex(string digits, out Color colour)
        {
            colour = Color.Empty;

            if (digits.Length != 6 && digits.Length != 8) return false;

            if (!uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packed)) return false;

            colour = digits.Length == 6
                ? Color.FromArgb(255, (int)(packed >> 16) & 0xFF, (int)(packed >> 8) & 0xFF, (int)packed & 0xFF)
                : Color.FromArgb(unchecked((int)packed));

            return true;
        }

        /// <summary>
        /// Reads one channel of a colour, which has to be a whole number from 0 to 255.
        /// </summary>
        private static bool TryParseChannel(string text, out int channel)
        {
            channel = 0;

            if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) return false;
            if (value < 0 || value > 255) return false;

            channel = value;
            return true;
        }

        /// <summary>
        /// Reads a colour as the packed number an older version of this program wrote, with
        /// or without the sign.
        /// </summary>
        /// <remarks>
        /// <see cref="Color.ToArgb"/> returns a signed 32 bit number, so the same colour
        /// written by hand or by another tool is usually the unsigned one. Both are read,
        /// so that a theme saved by the version which stored the number keeps its colours.
        /// </remarks>
        private static bool TryParseArgb(string text, out int argb)
        {
            argb = 0;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out argb)) return true;

            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var wide)) return false;

            // anything which does not fit the four channels is not a colour
            if (wide < int.MinValue || wide > uint.MaxValue) return false;

            argb = unchecked((int)(uint)wide);
            return true;
        }

        /// <summary>
        /// Reads a true or false value, also accepting the 1 and 0 a hand written file is
        /// likely to use.
        /// </summary>
        private static bool TryParseFlag(string text, out bool flag)
        {
            if (bool.TryParse(text, out flag)) return true;

            if (text == "1")
            {
                flag = true;
                return true;
            }

            if (text == "0")
            {
                flag = false;
                return true;
            }

            flag = false;
            return false;
        }

        /// <summary>
        /// Trims a name and keeps it to a length the theme list can show.
        /// </summary>
        private static string Normalise(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var trimmed = name.Trim();

            return trimmed.Length <= MaxNameLength ? trimmed : trimmed.Substring(0, MaxNameLength).TrimEnd();
        }
    }
}
