using AIHelper.Manage.Update.Targets.Mods.ModsInfos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIHelper.Manage.Update.Targets.Mods
{
    internal class ModInfo
    {
        internal string url;
        internal DirectoryInfo moddir;
    }

    class ModsMetaAuto : ModsBase
    {
        public ModsMetaAuto(updateInfo info) : base(info)
        {
        }

        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            var ModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            ModInfo modinfo = new ModInfo();

            List<ModsInfosBase> DBs = new List<ModsInfosBase>
            {
                new XUA(modinfo)
            };

            if (ModsList != null)
                foreach (var modname in ModsList)
                {
                    var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modname);

                    modinfo.moddir = new DirectoryInfo(ModPath);
                    modinfo.url = GetIMetaInfoUrl(ModPath);

                    if (!string.IsNullOrWhiteSpace(modinfo.url))
                    {
                        foreach(var db in DBs)
                        {
                            if (db.Check() && db.Owner.Length>0 && db.Project.Length>0)
                            {
                                var starts = db.StartsWith();
                                if (starts.Length > 0)
                                {
                                    infos.Add(modname, db.Owner+","+db.Project+","+ starts);
                                    break;
                                }
                            }
                        }
                    }
                }

            return infos;
        }

        private static string GetIMetaInfoUrl(string ModPath)
        {
            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (File.Exists(metaPath))
            {
                return ManageINI.GetINIValueIfExist(metaPath, "url", "General");
            }

            return "";
        }
    }
}
