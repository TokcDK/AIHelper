using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using AIHelper.Manage.Update.Targets.Mods.ModsMetaUrl;
using INIFileMan;

namespace AIHelper.Manage.Update.Targets.Mods
{
    internal class ModInfo
    {
        internal string Url;
        internal DirectoryInfo Moddir;
    }

    class ModsMeta : ModsBase
    {
        public ModsMeta(UpdateInfo info) : base(info)
        {
        }

        //ModInfo _targetinfo;
        //List<ModsMetaUrlBase> _dBs;

        /// <summary>
        /// Get enabled mods list infos from meta.ini of each mod
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            //_targetinfo = new ModInfo();

            //_dBs = new List<ModsMetaUrlBase>
            //{
            //    new Xua(_targetinfo)
            //};

            foreach (var modname in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(Info.UpdateOptions != null && Info.UpdateOptions.CheckEnabledModsOnlyCheckBox.Checked))
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modname);

                var modinfo = GetInfoFromMeta(modPath);

                if (string.IsNullOrWhiteSpace(modinfo)) continue;

                infos.Add(modPath, modinfo/*>*/.Replace(", ", ",")/*<fix of possible problems from space after ,*/);
            }

            return infos;
        }

        private string GetInfoFromMeta(string modPath)
        {
            var metaIniPath = Path.Combine(modPath, "meta.ini");
            if (!File.Exists(metaIniPath)) return "";

            var ini = ManageIni.GetINIFile(metaIniPath);

            foreach((string s, string k) in new[]
            {
                (ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName),
                ("General", "notes"),
            })
            {
                //Get by current source ID from notes
                if (!ini.KeyExists(k, s)) continue;
                var val = ini.GetKey(s, k);

                if (string.IsNullOrWhiteSpace(val)) continue;

                //updgit::BepInEx,BepInEx,BepInEx_x64::
                var updateInfoMatch = Regex.Match(val, $"{Info.SourceId}::([^:]+)::");

                if (!updateInfoMatch.Success) continue;
                if (string.IsNullOrWhiteSpace(updateInfoMatch.Value)) continue;
                if (!updateInfoMatch.Value.StartsWith(Info.SourceId,
                    System.StringComparison.InvariantCultureIgnoreCase)) continue;

                return updateInfoMatch.Groups[1].Value;
            }

            return "";
        }

        //List<Dictionary<string, string>> _dbData;
        //readonly string[] _dbDataParams = {
        //    "url.contains",
        //    "gitowner",
        //    "repository",
        //    "bepinexstarts",
        //    "bepinexends",
        //    "ipastarts",
        //    "ipaends",
        //    "gitversionfromfile",
        //    "skipif.contains.file0",
        //    "skipif.contains.file1",
        //    "skipif.contains.file2",
        //    "skipif.contains.file3",
        //    "skipif.contains.file4",
        //    "skipif.contains.file5"
        //};
        //private List<Dictionary<string, string>> GetDb()
        //{
        //    if (_dbData != null) return _dbData;

        //    _dbData = new List<Dictionary<string, string>>();
        //    var iniNum = 0;
        //    foreach (var iniFile in Directory.EnumerateFiles(ManageSettings.ModsUpdateDbInfoDir, "*.ini", SearchOption.AllDirectories))
        //    {
        //        INIFile ini = ManageIni.GetINIFile(iniFile);

        //        _dbData.Add(new Dictionary<string, string>());

        //        foreach (var setting in _dbDataParams)
        //        {
        //            var value = "";
        //            if (ini.KeyExists(setting)) value = ini.GetKey("", setting);

        //            _dbData[iniNum].Add(setting, value);
        //        }

        //        iniNum++;
        //    }

        //    return _dbData;
        //}
    }
}
