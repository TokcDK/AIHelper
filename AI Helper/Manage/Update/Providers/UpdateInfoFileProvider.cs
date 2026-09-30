using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace AIHelper.Manage.Update.Providers
{
    /// <summary>
    /// Reads the per game updateinfo.txt, a list of mod name / update record pairs.
    /// <para>
    /// Only mods that the active Mod Organizer profile actually contains are
    /// returned: a record for a mod that is not installed has nothing to update,
    /// and the folder path has to exist for the record to be usable at all.
    /// </para>
    /// </summary>
    internal sealed class UpdateInfoFileProvider : IUpdateRecordProvider
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Shape of a record line: a source id, a separator, anything but a colon,
        /// and the closing separator. A mod name never looks like this, which is
        /// what lets the reader tell the two kinds of line apart.
        /// </summary>
        private static readonly Regex RecordLineRegex = new Regex(
            "^[a-zA-Z0-9]+::[^:]+::$",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private readonly bool _onlyEnabledMods;

        /// <param name="onlyEnabledMods">
        /// When true, mods that are switched off in the profile are left alone.
        /// </param>
        internal UpdateInfoFileProvider(bool onlyEnabledMods)
        {
            _onlyEnabledMods = onlyEnabledMods;
        }

        public IEnumerable<UpdatableFolder> GetUpdatableFolders()
        {
            var result = new List<UpdatableFolder>();

            Dictionary<string, string> recordsByModName;
            try
            {
                recordsByModName = ReadRecordsByModName();
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read the update infos file '" + ManageSettings.UpdateInfosFilePath + "'.");
                return result;
            }

            if (recordsByModName.Count == 0) return result;

            foreach (var modName in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(_onlyEnabledMods))
            {
                string storedValue;
                if (!recordsByModName.TryGetValue(modName, out storedValue)) continue;

                GithubUpdateRecord record;
                if (!UpdateRecordParser.TryParseStoredValue(storedValue, out record))
                {
                    _log.Warn("Update: skipped '" + modName + "', the update info of the file is not readable: " + storedValue);
                    continue;
                }

                result.Add(new UpdatableFolder(
                    Path.Combine(ManageSettings.CurrentGameModsDirPath, modName),
                    record));
            }

            return result;
        }

        /// <summary>
        /// Reads the mod name / update record pairs of updateinfo.txt.
        /// <para>
        /// The file is a flat list: a mod name on one line, its record on the next.
        /// Blank lines and lines starting with ';' are comments. A mod name is kept
        /// exactly as written — Mod Organizer allows names with a leading space, and
        /// trimming one would make the name stop matching the folder.
        /// </para>
        /// </summary>
        /// <returns>Mod name to stored record. Empty when the file is missing.</returns>
        internal static Dictionary<string, string> ReadRecordsByModName()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var filePath = ManageSettings.UpdateInfosFilePath;
            if (!File.Exists(filePath)) return result;

            using (var reader = new StreamReader(filePath))
            {
                var modName = string.Empty;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart()[0] == ';') continue;

                    if (modName.Length == 0)
                    {
                        modName = line;
                        continue;
                    }

                    if (!RecordLineRegex.IsMatch(line)) continue;

                    // Indexer rather than Add: a mod listed twice in the file must
                    // not abort the whole read.
                    result[modName] = line;
                    modName = string.Empty;
                }
            }

            return result;
        }
    }
}
