using System;
using System.Collections.Generic;
using System.IO;
using AIHelper.Manage.Functions;
using INIFileMan;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class DevButtonData : IToolsTabButtonData, IButtonDataDev
    {
        public string Text => "Write rules info";

        public string Description => "Write mods rules info from rules file ti mods meta.ini";

        public bool IsVisible => File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "dev.txt"));

        public void OnClick(object o, EventArgs e)
        {
            //var s1 = "\x41e \x441\x431\x43e\x440\x43a\x435: \x414\x430\x43d\x43d\x430\x44f \x441\x431\x43e\x440\x43a\x430 \x441\x434\x435\x43b\x430\x43d\x430 \x43d\x430 \x431\x430\x437\x435 \x43f\x440\x43e\x433\x440\x430\x43c\x43c\x44b Mod Organizer.\x411\x43b\x430\x433\x43e\x434\x430\x440\x44f \x44d\x442\x43e\x43c\x443, \x43c\x43e\x434\x43f\x430\x43a\x438 \x438 \x43f\x43b\x430\x433\x438\x43d\x44b \x432 \x441\x431\x43e\x440\x43a\x435 \x44f\x432\x43b\x44f\x44e\x442\x441\x44f \x43e\x442\x434\x435\x43b\x44c\x43d\x44b\x43c\x438 \x43a\x43e\x43c\x43f\x43e\x43d\x435\x43d\x442\x430\x43c\x438 \x438 \x43b\x435\x436\x430\x442 \x43e\x442\x434\x435\x43b\x44c\x43d\x43e \x43e\x442 \x438\x433\x440\x44b, \x43a\x430\x436\x434\x44b\x439 \x432 \x441\x432\x43e\x435\x439 \x43f\x430\x43f\x43a\x435.\x427\x442\x43e \x43f\x43e\x437\x432\x43e\x43b\x44f\x435\x442 \x432\x43a\x43b\x44e\x447\x430\x442\x44c/\x43e\x442\x43a\x43b\x44e\x447\x430\x442\x44c (\x438 \x43f\x440\x438 \x43d\x435\x43e\x431\x445\x43e\x434\x438\x43c\x43e\x441\x442\x438, \x443\x434\x430\x43b\x44f\x442\x44c) \x438\x445 \x43f\x430\x440\x43e\x439 \x43a\x43b\x438\x43a\x43e\x432, \x447\x435\x440\x435\x437 \x43c\x435\x43d\x44e \x43f\x440\x43e\x433\x440\x430\x43c\x43c\x44b Mod Organizer.\x41f\x440\x438\x43c\x435\x440 \x441\x431\x43e\x440\x43a\x438 \x438\x433\x440\x44b, \x441 \x443\x43f\x440\x430\x432\x43b\x435\x43d\x438\x435\x43c \x43c\x43e\x434\x430\x43c\x438 \x447\x435\x440\x435\x437 MO:\xa0\x420\x430\x437\x434\x430\x447\x430 Honey Select \x43e\x442 bigbigbos";
            //var s2 = s1.ToHex();
            //foreach(Match m in Regex.Matches(s1, @"\\x[0-9a-f]{3}"))
            //{
            //    var ssss = m.Value.UnHex();
            //}
            //var s3 = s2.UnHex();
            var targetIniPath = Path.Combine(ManageSettings.CurrentGameDirPath, "Meta.ini");

            //if (File.Exists(targetIniPath)) return;

            var targetIni = new INIFile(targetIniPath, true);

            var updateInfos = new Dictionary<string, string>();
            if (File.Exists(ManageSettings.UpdateInfosFilePath))
            {
                updateInfos = ManageUpdateMods.GetUpdateInfosFromFile("updgit");
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
