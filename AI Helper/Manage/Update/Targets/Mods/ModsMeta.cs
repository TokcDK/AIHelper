using AIHelper.Manage.Update.Targets.Mods.ModsMetaUrl;
using INIFileMan;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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

        ModInfo _targetinfo;
        List<ModsMetaUrlBase> _dBs;

        /// <summary>
        /// Get enabled mods list infos from meta.ini of each mod
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            _targetinfo = new ModInfo();

            _dBs = new List<ModsMetaUrlBase>
            {
                new Xua(_targetinfo)
            };

            foreach (var modname in ManageModOrganizer.EnumerateModNamesListFromActiveMoProfile(ManageSettings.MainForm.CheckEnabledModsOnlyLabel.IsChecked()))
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modname);

                var modinfo = GetInfoFromMeta(modPath);

                if (!string.IsNullOrWhiteSpace(modinfo))
                {
                    infos.Add(modPath, modinfo/*>*/.Replace(", ", ",")/*<fix of possible problems from space after ,*/);
                }
            }

            return infos;
        }

        private string GetInfoFromMeta(string modPath)
        {
            var metaIniPath = Path.Combine(modPath, "meta.ini");
            if (!File.Exists(metaIniPath))
            {
                return "";
            }

            var ini = ManageIni.GetINIFile(metaIniPath);

            //Get by current source ID from notes
            string val = "";
            if (ini.KeyExists("notes", "General"))
            {
                val = ini.GetKey("General", "notes");
            }

            if (string.IsNullOrWhiteSpace(val))
                return "";

            //updgit::BepInEx,BepInEx,BepInEx_x64::
            var updateInfo = Regex.Match(val, Info.SourceId + "::([^:]+)::");

            if (updateInfo.Success && !string.IsNullOrWhiteSpace(updateInfo.Value) && updateInfo.Value.StartsWith(Info.SourceId, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return updateInfo.Result("$1");
            }
            else
            {
                //read info from standalone key
                //need to think about section and key names
                if (ini.SectionExistsAndNotEmpty(ManageSettings.AiMetaIniSectionName))
                {
                    if (ini.KeyExists(ManageSettings.AiMetaIniKeyUpdateName, ManageSettings.AiMetaIniSectionName))
                    {
                        var info = ini.GetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName);

                        return Regex.Match(info, Info.SourceId + "::([^:]+)::").Result("$1");
                    }
                }
            }

            // Get from meta.ini url
            //val = "";
            //if (INI.KeyExists("url", "General"))
            //{
            //    val = INI.ReadINI("General", "url");
            //}

            //if (!string.IsNullOrWhiteSpace(val))
            //{
            //    targetinfo.moddir = new DirectoryInfo(ModPath);
            //    targetinfo.url = val;

            //    //hardcoded
            //    //foreach (var db in DBs)
            //    //{
            //    //    if (db.Check() && db.Owner.Length > 0 && db.Project.Length > 0)
            //    //    {
            //    //        var starts = db.StartsWith();
            //    //        if (starts.Length > 0)
            //    //        {
            //    //            return db.Owner + "," + db.Project + "," + starts;
            //    //        }
            //    //    }
            //    //}

            //    //from ini plugins
            //    foreach (var db in GetDB())
            //    {
            //        if (targetinfo.url.Contains(db["url.contains"]) && !ContainsFile(db) && db["gitowner"].Length > 0 && db["repository"].Length > 0)
            //        {
            //            var parts = GetFilePartsInfo(db);

            //            if (parts[0].Length > 0)
            //            {
            //                return db["gitowner"] + "," + db["repository"] + "," + parts[0] + "," + parts[1].Trim() + "," + db["gitversionfromfile"];
            //            }
            //        }
            //    }
            //}

            return "";
        }

        /// <summary>
        /// true when folder contains file from db
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private bool ContainsFile(Dictionary<string, string> db)
        {
            var i = 0;
            while (db.ContainsKey("skipif.contains.file" + i) && !string.IsNullOrWhiteSpace(db["skipif.contains.file" + i]))
            {
                if (File.Exists(Path.GetFullPath(Path.Combine(_targetinfo.Moddir.FullName, db["skipif.contains.file" + i]))))
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        private string[] GetFilePartsInfo(Dictionary<string, string> db)
        {
            if (Directory.Exists(Path.Combine(_targetinfo.Moddir.FullName, "BepInEx")))
            {
                return new[] { db["bepinexstarts"], db["bepinexends"] };
            }
            else
            {
                return new[] { db["ipastarts"], db["ipaends"] };
            }
        }

        List<Dictionary<string, string>> _dbData;
        readonly string[] _dbDataParams = {
            "url.contains",
            "gitowner",
            "repository",
            "bepinexstarts",
            "bepinexends",
            "ipastarts",
            "ipaends",
            "gitversionfromfile",
            "skipif.contains.file0",
            "skipif.contains.file1",
            "skipif.contains.file2",
            "skipif.contains.file3",
            "skipif.contains.file4",
            "skipif.contains.file5"
        };
        private List<Dictionary<string, string>> GetDb()
        {
            if (_dbData == null)
            {
                _dbData = new List<Dictionary<string, string>>();
                var iniNum = 0;
                foreach (var iniFile in Directory.EnumerateFiles(ManageSettings.ModsUpdateDbInfoDir, "*.ini", SearchOption.AllDirectories))
                {
                    INIFile ini = ManageIni.GetINIFile(iniFile);

                    _dbData.Add(new Dictionary<string, string>());

                    foreach (var setting in _dbDataParams)
                    {
                        var value = "";
                        if (ini.KeyExists(setting))
                        {
                            value = ini.GetKey("", setting);
                        }
                        _dbData[iniNum].Add(setting, value);
                    }

                    iniNum++;
                }
            }

            return _dbData;
        }
    }
}
