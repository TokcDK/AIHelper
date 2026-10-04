using AIHelper.Manage.Rules.ModList;
using INIFileMan;
using System.IO;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    /// <summary>
    /// Writes the modlist rules the fix has worked out into meta.ini, unless the mod
    /// already carries them.
    /// </summary>
    class WriteInfoFromFileifMissing : ModlistDataMetaIniFixesBase
    {
        /// <param name="modlistData">The rules as they stand after the fixes were applied.</param>
        /// <param name="ini">The mod's meta.ini.</param>
        /// <param name="mod">The mod's folder.</param>
        public WriteInfoFromFileifMissing(ModListData modlistData, INIFile ini, string mod) : base(modlistData, ini, mod)
        {
        }

        internal override bool Apply()
        {
            var section = ManageSettings.AiMetaIniSectionName;
            var key = ManageSettings.AiMetaIniKeyModlistRulesInfoName;
            var modName = Path.GetFileName(mod);

            if (modlistData == null || modlistData.RulesDict == null) return false;
            if (!modlistData.RulesDict.ContainsKey(modName)) return false;
            if (ini.SectionExists(section) && ini.KeyExists(key, section)) return false;

            ini.SetKey(section, key, "::mlinfo::\\r\\n" + string.Join("\\r\\n", modlistData.RulesDict[modName]) + "\\r\\n::", false);

            modlistData.Report.Add(modName + ": " + T._("added rules info into meta.ini"));

            return true;
        }
    }
}
