﻿using AIHelper.Manage.Rules.ModList;
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
            if (!ini.KeyExists("gameName", "General") || (/*gameName =*/ ini.GetKey("General", "gameName")) == ManageSettings.GetMoCurrentGameName())
            {
                return false;
            }

            ini.SetKey("General", "gameName", ManageSettings.GetMoCurrentGameName(), false);

            modlistData.Report.Add(Path.GetFileName(mod) + ": " + T._("fixed game name"));

            return true;
        }
    }
}