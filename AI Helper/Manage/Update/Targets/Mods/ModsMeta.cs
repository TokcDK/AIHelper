using AIHelper.Manage.Update.Targets.Mods.ModsMetaUrl;
using INIFileMan;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update.Targets.Mods
{
    internal class ModInfo
    {
        internal string url;
        internal DirectoryInfo moddir;
    }

    class ModsMeta : ModsBase
    {
        public ModsMeta(updateInfo info) : base(info)
        {
        }

        ModInfo targetinfo;
        List<ModsMetaUrlBase> DBs;

        /// <summary>
        /// Get enabled mods list infos from meta.ini of each mod
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            var ModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            targetinfo = new ModInfo();

            DBs = new List<ModsMetaUrlBase>
            {
                new XUA(targetinfo)
            };

            if (ModsList != null)
                foreach (var modname in ModsList)
                {
                    var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modname);

                    var modinfo = GetInfoFromMeta(ModPath);

                    if (!string.IsNullOrWhiteSpace(modinfo))
                    {
                        infos.Add(ModPath, modinfo/*>*/.Replace(", ",",")/*<fix of possible problems from space after ,*/);
                    }
                }

            return infos;
        }

        private string GetInfoFromMeta(string ModPath)
        {
            var MetaINIPath = Path.Combine(ModPath, "meta.ini");
            if (!File.Exists(MetaINIPath))
            {
                return "";
            }

            var INI = new INIFile(MetaINIPath);

            //Get by current source ID from notes
            string val = "";
            if (INI.KeyExists("notes", "General"))
            {
                val = INI.ReadINI("General", "notes");
            }

            if (string.IsNullOrWhiteSpace(val))
                return "";

            //updgit::BepInEx,BepInEx,BepInEx_x64::
            var UpdateInfo = Regex.Match(val, info.SourceID + "::([^:]+)::");

            if (UpdateInfo.Success && !string.IsNullOrWhiteSpace(UpdateInfo.Value) && UpdateInfo.Value.StartsWith(info.SourceID, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return UpdateInfo.Result("$1");
            }
            else
            {
                //read info from standalone key
                //need to think about section and key names
                if (INI.SectionExistsAndNotEmpty(ManageSettings.AIMetaINISectionName()))
                {
                    if (INI.KeyExists(ManageSettings.AIMetaINIKeyUpdateName(), ManageSettings.AIMetaINISectionName()))
                    {
                        var Info = INI.ReadINI(ManageSettings.AIMetaINISectionName(), ManageSettings.AIMetaINIKeyUpdateName());
                                                
                        return Regex.Match(Info, info.SourceID + "::([^:]+)::").Result("$1");
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
                if(File.Exists(Path.GetFullPath(Path.Combine(targetinfo.moddir.FullName, db["skipif.contains.file" + i]))))
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        private string[] GetFilePartsInfo(Dictionary<string, string> db)
        {
            if (Directory.Exists(Path.Combine(targetinfo.moddir.FullName, "BepInEx")))
            {
                return new[] { db["bepinexstarts"], db["bepinexends"] };
            }
            else
            {
                return new[] { db["ipastarts"], db["ipaends"] };
            }
        }

        List<Dictionary<string, string>> DBData;
        string[] DBDataParams = new string[]
        {
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
        private List<Dictionary<string, string>> GetDB()
        {
            if (DBData == null)
            {
                DBData = new List<Dictionary<string, string>>();
                var iniNum = 0;
                foreach (var ini in Directory.EnumerateFiles(ManageSettings.GetModsUpdateDBInfoDir(), "*.ini", SearchOption.AllDirectories))
                {
                    INIFile INI = new INIFile(ini);

                    DBData.Add(new Dictionary<string, string>());

                    foreach (var setting in DBDataParams)
                    {
                        var value = "";
                        if (INI.KeyExists(setting))
                        {
                            value = INI.ReadINI("", setting);
                        }
                        DBData[iniNum].Add(setting, value);
                    }

                    iniNum++;
                }
            }

            return DBData;
        }
    }
}
