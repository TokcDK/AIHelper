using AIHelper.Manage.Update.Providers;
using INIFileMan;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class DevButtonData : IToolsTabButtonData, IButtonDataDev
    {
        public string Text => "Write rules info";

        public string Description => "Write mods rules info from rules file ti mods meta.ini";

        public bool IsVisible => File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "dev.txt"));

        public void OnClick(object o, EventArgs e)
        {
            var targetIniPath = Path.Combine(ManageSettings.CurrentGameDirPath, "Meta.ini");

            var targetIni = new INIFile(targetIniPath, true);

            var updateInfos = new Dictionary<string, string>();
            if (File.Exists(ManageSettings.UpdateInfosFilePath))
            {
                updateInfos = UpdateInfoFileProvider.ReadRecordsByModName();
            }

            foreach (var mod in ManageModOrganizer.GetModNamesListFromActiveMoProfile(false))
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, mod);
                if (!Directory.Exists(modPath)) continue;

                var metaPath = Path.Combine(modPath, "meta.ini");
                if (!File.Exists(metaPath)) continue;

                var ini = new INIFile(metaPath);

                var targetSectionName = mod;

                if (ini.KeyExists("notes", "General"))
                {
                    var value = ini.GetKey("General", "notes").StripMONotesHTML();
                    targetIni.SetKey(targetSectionName, "notes", value, false);
                }

                if (ini.KeyExists("url", "General"))
                {
                    targetIni.SetKey(targetSectionName, "url", ini.GetKey("General", "url"), false);
                }

                if (ini.KeyExists("ModUpdateInfo", "AISettings"))
                {
                    targetIni.SetKey(targetSectionName, "updateinfo", ini.GetKey("AISettings", "ModUpdateInfo"), false);
                }
                else if (updateInfos.ContainsKey(mod))
                {
                    targetIni.SetKey(targetSectionName, "updateinfo", updateInfos[mod], false);
                    ini.SetKey("AISettings", "ModUpdateInfo", updateInfos[mod]);
                }

                if (ini.KeyExists("ModlistRulesInfo", "AISettings"))
                {
                    targetIni.SetKey(targetSectionName, "modlistrules", ini.GetKey("AISettings", "ModlistRulesInfo"), false);
                }
            }

            targetIni.WriteFile();
        }
    }
}
