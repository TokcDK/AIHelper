using AIHelper.Manage.Rules.ModList;
using INIFileMan;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class MoveAIhelperInfosFromMetaIniNotesToKey : ModlistDataMetaIniFixesBase
    {
        /// <summary>
        /// fix for incorrect gameName set for the mod
        /// </summary>
        /// <param name="modlistData"></param>
        /// <param name="ini"></param>
        /// <param name="mod"></param>
        public MoveAIhelperInfosFromMetaIniNotesToKey(ModListData modlistData, INIFile ini, string mod) : base(modlistData, ini, mod)
        {
        }

        internal override bool Apply()
        {
            if (!ini.KeyExists("notes", "General"))
            {
                return true;
            }

            var metanotes = ini.GetKey("General", "notes");
            if (string.IsNullOrWhiteSpace(metanotes))
            {
                return true;
            }

            var patternsOfInfoForMove = new string[2][]
            {
                                        new string[2] { "mlinfo", ManageSettings.AiMetaIniKeyModlistRulesInfoName() },//regex to capture ::mlinfo:: with html tags
                                        new string[2] { "updgit", ManageSettings.AiMetaIniKeyUpdateName() }//regex to capture update info with html tags
            };

            var metainiinfomoved = false;
            foreach (var pattern in patternsOfInfoForMove)
            {
                var regex = @"<p style\=[^>]*>(::)?" + pattern[0] + @"::(?:(?!::).)+::<\/p>";//regex to capture info with html tags
                var info = Regex.Match(metanotes, regex);
                if (info.Success && !string.IsNullOrWhiteSpace(info.Value))
                {
                    var infoValue = Regex.Replace(info.Value.Replace(@"\n", @"\r\n"), "<[^>]*>", "");//cleaned info from html tags
                                                                                                     //write new key to meta ini with info
                    ini.SetKey(ManageSettings.AiMetaIniSectionName(), pattern[1], infoValue, false);

                    modlistData.Report.Add(Path.GetFileName(mod) + ": " + T._("moved meta ini info from notes in ini key") + " " + pattern[1]);

                    metainiinfomoved = true;
                    metanotes = metanotes.Replace(info.Value, "");//remove info from notes after it was set to ini key
                }
            }

            if (metainiinfomoved)
            {
                ini.SetKey("General", "notes", metanotes, false);//write notes with removed mlinfo
                return true;
            }

            return false;
        }
    }
}
