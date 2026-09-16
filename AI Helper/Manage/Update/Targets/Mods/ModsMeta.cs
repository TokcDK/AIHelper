using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update.Targets.Mods
{
    /// <summary>
    /// Target for mods that carry their update record in meta.ini, either in the
    /// AIHelper section or in the notes written by Mod Organizer.
    /// </summary>
    class ModsMeta : ModsBase
    {
        /// <summary>
        /// Sections and keys that may hold the update record, in the order they
        /// are searched.
        /// </summary>
        static readonly (string Section, string Key)[] UpdateRecordLocations =
        {
            (ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName),
            ("General", "notes"),
        };

        public ModsMeta(UpdateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Gets the enabled mods whose meta.ini contains an update record for
        /// the current source.
        /// </summary>
        /// <returns>Mod folder path to update record pairs.</returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            var onlyEnabledMods = Info.UpdateOptions != null && Info.UpdateOptions.CheckEnabledModsOnlyCheckBox.Checked;

            foreach (var modname in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(onlyEnabledMods))
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modname);

                var modinfo = GetInfoFromMeta(modPath);

                if (string.IsNullOrWhiteSpace(modinfo)) continue;

                // indexer instead of Add: a duplicated mod name must not throw
                infos[modPath] = NormalizeUpdateRecord(modinfo);
            }

            return infos;
        }

        /// <summary>
        /// Reads the update record of the current source from a mod's meta.ini.
        /// </summary>
        /// <param name="modPath">Mod folder.</param>
        /// <returns>The record without the source prefix, or an empty string.</returns>
        private string GetInfoFromMeta(string modPath)
        {
            var metaIniPath = Path.Combine(modPath, MetaIniFileName);
            if (!File.Exists(metaIniPath)) return "";

            var ini = ManageIni.GetINIFile(metaIniPath);

            foreach (var location in UpdateRecordLocations)
            {
                if (!ini.KeyExists(location.Key, location.Section)) continue;

                var val = ini.GetKey(location.Section, location.Key);

                if (string.IsNullOrWhiteSpace(val)) continue;

                // the record looks like 'updgit::BepInEx,BepInEx,BepInEx_x64::'
                // the source id is escaped because it is data, not a pattern
                var updateInfoMatch = Regex.Match(val, Regex.Escape(Info.SourceId) + "::(.+?)::");

                if (!updateInfoMatch.Success) continue;
                if (string.IsNullOrWhiteSpace(updateInfoMatch.Value)) continue;

                return updateInfoMatch.Groups[1].Value;
            }

            return "";
        }

        /// <summary>
        /// Normalises the separators of an update record so a record written by
        /// hand with a space after each comma still parses.
        /// </summary>
        /// <param name="updateRecord">Record read from meta.ini.</param>
        private static string NormalizeUpdateRecord(string updateRecord)
        {
            return updateRecord.Replace(", ", ",");
        }
    }
}
