using AIHelper.Manage.Rules.ModList;
using INIFileMan;
using System.IO;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class WriteInfoFromFileifMissing : ModlistDataMetaIniFixesBase
    {
        /// <summary>
        /// fix for incorrect gameName set for the mod
        /// </summary>
        /// <param name="modlistData"></param>
        /// <param name="ini"></param>
        /// <param name="mod"></param>
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

    public class ModRules
    {
        public string ModDirName { get; set; }
        public string Rules { get; set; }
    }
}
