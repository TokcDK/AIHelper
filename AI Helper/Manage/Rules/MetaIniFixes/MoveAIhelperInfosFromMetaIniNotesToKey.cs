using AIHelper.Manage.Rules.ModList;
using INIFileMan;
using System.IO;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class FixIncorrecModGameNameSet : ModlistDataMetaIniFixesBase
    {
        /// <summary>
        /// fix for incorrect gameName set for the mod
        /// </summary>
        /// <param name="modlistData"></param>
        /// <param name="ini"></param>
        /// <param name="mod"></param>
        public FixIncorrecModGameNameSet(ModListData modlistData, INIFile ini, string mod) : base(modlistData, ini, mod)
        {
        }

        internal override bool Apply()
        {
            var gameName = ManageSettings.GetMoCurrentGameName();
            if (string.IsNullOrWhiteSpace(gameName)) return false;
            if (ini.KeyExists("gameName", "General") && ini.GetKey("General", "gameName") == gameName) return false;

            ini.SetKey("General", "gameName", gameName, false);

            modlistData.Report.Add(Path.GetFileName(mod) + ": " + T._("fixed game name"));

            return true;
        }
    }
}
