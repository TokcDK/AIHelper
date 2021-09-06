using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Update.Targets.Mods
{
    class ModsList : ModsBase
    {
        public ModsList(UpdateInfo info) : base(info)
        {
        }

        readonly string _updateInfosFile = ManageSettings.GetUpdateInfosFilePath();

        /// <summary>
        /// Get enabled and exists Mods list from updateinfos file
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();
            string[] modsList = null;
            try
            {
                modsList = ManageModOrganizer.GetModNamesListFromActiveMoProfile(SharedData.GameData.MainForm.CheckEnabledModsOnlyLabel.IsChecked());
                var updateInfoList = GetUpdateInfosFromFile();

                if (updateInfoList != null && updateInfoList.Count > 0)
                    foreach (var modname in modsList)
                    {
                        var modPath = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modname);
                        if (updateInfoList.ContainsKey(modname) && !infos.ContainsKey(modPath))
                        {
                            infos.Add(modname, updateInfoList[modname]);
                        }
                    }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error while get update infos:\r\n" + ex + "\r\ninfos count=" + infos.Count + (modsList != null ? "\r\nModsList count=" + modsList.Length : "ModsList is null"));
            }

            return infos;
        }

        private Dictionary<string, string> GetUpdateInfosFromFile()
        {
            var d = new Dictionary<string, string>();

            if (!File.Exists(_updateInfosFile))
                return null;

            using (StreamReader sr = new StreamReader(_updateInfosFile))
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
                        if (pair[1].StartsWith(Info.SourceId, System.StringComparison.InvariantCultureIgnoreCase)
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
