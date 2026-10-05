using System;
using INIFileMan;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Stores the update record in a mod's meta.ini, in the place
    /// <see cref="Providers.MetaIniUpdateRecordProvider"/> reads it from.
    /// <para>
    /// The record and the url are written together because they describe the same
    /// thing and a reader may find either: the record is what the updater reads, and
    /// the url is what Mod Organizer shows. A url which is already there is left
    /// alone, so a link the user typed by hand is not replaced by the one derived
    /// from the record.
    /// </para>
    /// <para>
    /// The section and key names live here rather than in the editor, so that the
    /// place the record is stored is written down once and read from one place.
    /// </para>
    /// </summary>
    internal static class MetaIniUpdateRecordWriter
    {
        /// <summary>Section and key Mod Organizer keeps the mod url in.</summary>
        private const string UrlSection = "General";
        private const string UrlKey = "url";

        /// <summary>
        /// Stores the record and, when it is missing, the url.
        /// </summary>
        /// <param name="ini">meta.ini of the mod.</param>
        /// <param name="site">Host the record points at, for example github.com.</param>
        /// <param name="record">Record to store.</param>
        /// <returns>True when anything was written to the file.</returns>
        internal static bool Write(INIFile ini, string site, GithubUpdateRecord record)
        {
            if (ini == null || record == null) return false;

            var changed = false;

            // A record needs a file name prefix to be usable, and storing a thinner one
            // would only produce something every reader has to reject again.
            if (record.IsValid)
            {
                var newInfo = UpdateRecordParser.Format(record);
                var currentInfo = ini.GetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName);

                if (!IsSameRecord(currentInfo, newInfo))
                {
                    ini.SetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName, newInfo, false);
                    changed = true;
                }
            }

            if (record.Owner.Length > 0 && record.Repository.Length > 0
                && string.IsNullOrEmpty(ini.GetKey(UrlSection, UrlKey)))
            {
                ini.SetKey(UrlSection, UrlKey,
                    GithubUrl.BuildRepositoryUrl(site, record.Owner, record.Repository), false);
                changed = true;
            }

            // The two keys are written at once: the file is rewritten as a whole either
            // way, so writing it after each of them would only do the same work twice.
            if (changed) ini.WriteFile();

            return changed;
        }

        /// <summary>
        /// True when the stored value already is the record, allowing for the spaces an
        /// older version of the application put after each comma.
        /// </summary>
        private static bool IsSameRecord(string currentInfo, string newInfo)
        {
            return string.Equals(currentInfo, newInfo, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(currentInfo.Replace(", ", ","), newInfo, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
