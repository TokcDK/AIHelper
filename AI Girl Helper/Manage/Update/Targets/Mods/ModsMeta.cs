using AIHelper.Manage.Update.Targets.Mods.ModsMetaUrl;
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
                        infos.Add(ModPath, modinfo);
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

            Manage.INIFile INI = new Manage.INIFile(MetaINIPath);

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

            // Get from meta.ini url
            val = "";
            if (INI.KeyExists("url", "General"))
            {
                val = INI.ReadINI("General", "url");
            }

            if (!string.IsNullOrWhiteSpace(val))
            {
                targetinfo.moddir = new DirectoryInfo(ModPath);
                targetinfo.url = val;

                foreach (var db in DBs)
                {
                    if (db.Check() && db.Owner.Length > 0 && db.Project.Length > 0)
                    {
                        var starts = db.StartsWith();
                        if (starts.Length > 0)
                        {
                            return db.Owner + "," + db.Project + "," + starts;
                        }
                    }
                }
            }

            return "";
        }
    }
}
