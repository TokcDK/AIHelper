using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Update.Providers
{
    /// <summary>
    /// Reads the update record a mod carries in its own meta.ini.
    /// <para>
    /// meta.ini is where Mod Organizer keeps everything it knows about a mod, and
    /// it is the right place for the record: the file already exists in every mod
    /// folder, and Mod Organizer excludes it from conflict checking, so putting the
    /// record there cannot make two mods look like they overwrite each other.
    /// </para>
    /// </summary>
    internal sealed class MetaIniUpdateRecordProvider : IUpdateRecordProvider
    {
        /// <summary>File Mod Organizer keeps its per mod data in.</summary>
        private const string MetaIniFileName = "meta.ini";

        /// <summary>
        /// Places the record may be kept, in the order they are searched: the
        /// section this application writes, then the notes Mod Organizer shows to
        /// the user, where records written by hand live.
        /// </summary>
        private static readonly (string Section, string Key)[] RecordLocations =
        {
            (ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName),
            ("General", "notes"),
        };

        private readonly bool _onlyEnabledMods;

        /// <param name="onlyEnabledMods">
        /// When true, mods that are switched off in the profile are left alone.
        /// </param>
        internal MetaIniUpdateRecordProvider(bool onlyEnabledMods)
        {
            _onlyEnabledMods = onlyEnabledMods;
        }

        public IEnumerable<UpdatableFolder> GetUpdatableFolders()
        {
            var result = new List<UpdatableFolder>();

            foreach (var modName in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(_onlyEnabledMods))
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);

                GithubUpdateRecord record;
                if (!TryReadRecord(modPath, out record)) continue;

                result.Add(new UpdatableFolder(modPath, record));
            }

            return result;
        }

        /// <summary>
        /// Reads the update record of a mod, if it has one.
        /// </summary>
        /// <param name="modPath">Mod folder.</param>
        /// <param name="record">Record that was found.</param>
        /// <returns>True when the mod carries a parsable record.</returns>
        private static bool TryReadRecord(string modPath, out GithubUpdateRecord record)
        {
            record = null;

            var metaIniPath = Path.Combine(modPath, MetaIniFileName);
            if (!File.Exists(metaIniPath)) return false;

            var ini = ManageIni.GetINIFile(metaIniPath);

            foreach (var location in RecordLocations)
            {
                if (!ini.KeyExists(location.Key, location.Section)) continue;

                if (UpdateRecordParser.TryExtract(ini.GetKey(location.Section, location.Key), out record))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
