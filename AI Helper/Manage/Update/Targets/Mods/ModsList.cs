using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Update.Targets.Mods
{
    class ModsList : ModsBase
    {
        public ModsList(updateInfo info) : base(info)
        {
        }

        readonly string updateInfosFile = ManageSettings.GetUpdateInfosFilePath();

        /// <summary>
        /// Get enabled and exists Mods list from updateinfos file
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();
            string[] ModsList = null;
            try
            {
                ModsList = ManageMO.GetModNamesListFromActiveMOProfile();
                var updateInfoList = GetUpdateInfosFromFile();

                if (updateInfoList != null && updateInfoList.Count > 0)
                    foreach (var modname in ModsList)
                    {
                        var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modname);
                        if (updateInfoList.ContainsKey(modname) && !infos.ContainsKey(ModPath))
                        {
                            infos.Add(modname, updateInfoList[modname]);
                        }
                    }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error while get update infos:\r\n" + ex + "\r\ninfos count=" + infos.Count + (ModsList != null ? "\r\nModsList count=" + ModsList.Length : "ModsList is null"));
            }

            return infos;
        }

        private Dictionary<string, string> GetUpdateInfosFromFile()
        {
            var d = new Dictionary<string, string>();

            if (!File.Exists(updateInfosFile))
                return null;

            using (StreamReader sr = new StreamReader(updateInfosFile))
            {
                string line;
                string[] pair = new string[2];
                pair[0] = "";
                pair[1] = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart()[0] == ';')
                        continue;

                    if (pair[0].Length == 0)
                    {
                        pair[0] = line;
                    }
                    else if (pair[1].Length == 0)
                    {
                        pair[1] = line;
                    }

                    if (pair[0].Length > 0 && pair[1].Length > 0)
                    {
                        if (pair[1].StartsWith(info.SourceID, System.StringComparison.InvariantCultureIgnoreCase)
                        && !d.ContainsKey(pair[0]))
                        {
                            d.Add(pair[0], pair[1]);

                        }
                        else
                        {
                            pair[0] = "";
                            pair[1] = "";
                        }
                    }
                }
            }

            return d;
        }
    }
}
