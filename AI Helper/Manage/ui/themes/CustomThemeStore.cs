using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The roles of a palette, discovered rather than listed.
        /// </summary>
        /// <remarks>
        /// Reflection on purpose. A role added to <see cref="ThemePalette"/> is then
        /// saved and read back without anything here being touched, and the failure mode
        /// of forgetting to update a hand written list is a colour which silently
        /// reverts to a derived one every time the application restarts.
        /// </remarks>
        private static readonly PropertyInfo[] PaletteRoles = typeof(ThemePalette)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite)
            .Where(property => property.PropertyType == typeof(Color) || property.PropertyType == typeof(bool))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

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

                ini.SetKey(SectionName, NameKey, theme.Name, false);
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

        private static void WritePalette(INIFile ini, ThemePalette palette)
        {
            foreach (var role in PaletteRoles)
            {
                var value = role.GetValue(palette, null);
                string text;

                if (role.PropertyType == typeof(Color))
                {
                    // the whole ARGB value: a role may carry an alpha, and dropping it
                    // would change the colour rather than only its spelling
                    text = ((Color)value).ToArgb().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    text = ((bool)value).ToString(CultureInfo.InvariantCulture);
                }

                ini.SetKey(SectionName, role.Name, text, false);
            }
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

            foreach (var role in PaletteRoles)
            {
                if (!ini.KeyExists(role.Name, SectionName)) continue;

                var text = ini.GetKey(SectionName, role.Name);

                if (role.PropertyType == typeof(Color))
                {
                    if (!TryParseArgb(text, out var argb))
                    {
                        _log.Warn("Role '" + role.Name + "' of the theme file holds '" + text
                            + "', which is not a colour, and is left out: " + path);
                        continue;
                    }

                    role.SetValue(palette, Color.FromArgb(argb), null);
                }
                else
                {
                    if (!TryParseFlag(text, out var flag))
                    {
                        _log.Warn("Role '" + role.Name + "' of the theme file holds '" + text
                            + "', which is not true or false, and is left out: " + path);
                        continue;
                    }

                    role.SetValue(palette, flag, null);
                }
            }

            return palette.Complete();
        }

        /// <summary>
        /// Reads a colour value, with or without the sign.
        /// </summary>
        /// <remarks>
        /// The writer stores what <see cref="Color.ToArgb"/> returns, which is a signed
        /// 32 bit number. The same colour written by hand or by another tool is usually the
        /// unsigned one, and refusing it would drop the role and put that part of the
        /// window back on a derived colour without anything saying so.
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
