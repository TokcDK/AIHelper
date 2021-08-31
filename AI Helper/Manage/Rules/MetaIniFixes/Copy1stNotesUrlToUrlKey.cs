using AIHelper.Manage.Rules.ModList;
using INIFileMan;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class Copy1stNotesUrlToUrlKey : ModlistDataMetaIniFixesBase
    {
        /// <summary>
        /// Copy 1st found url from notes to url key
        /// </summary>
        /// <param name="modlistData"></param>
        /// <param name="ini"></param>
        /// <param name="mod"></param>
        public Copy1stNotesUrlToUrlKey(ModListData modlistData, INIFile ini, string mod) : base(modlistData, ini, mod)
        {
        }

        internal override bool Apply()
        {
            if (ini.KeyExists("url", "General") && !string.IsNullOrWhiteSpace(ini.GetKey("General", "url")))
            {
                //set hasCustomURL to true if url exists and hasCustomURL is false
                if (!ini.KeyExists("hasCustomURL", "General") || ini.GetKey("General", "hasCustomURL").Length == 5/*=="false"*/)
                {
                    ini.SetKey("General", "hasCustomURL", "true", false);
                    return true;
                }
            }
            else// if (!INI.KeyExists("url", "General") || string.IsNullOrWhiteSpace(INI.ReadINI("General", "url")))
            {
                var metanotes = ini.GetKey("General", "notes");
                if (!string.IsNullOrWhiteSpace(metanotes))
                {
                    var regex = @"<a href\=\\""[^>]+\\"">";//pattern for url inside notes
                    var url = Regex.Match(metanotes, regex);
                    if (url.Success && !string.IsNullOrWhiteSpace(url.Value))
                    {
                        var urlValue = url.Value.Remove(url.Value.Length - 3, 3).Remove(0, 10);
                        ini.SetKey("General", "url", urlValue, false);
                        ini.SetKey("General", "hasCustomURL", "true", false);


                        modlistData.Report.Add(Path.GetFileName(mod) + ": " + T._("added url from notes"));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
