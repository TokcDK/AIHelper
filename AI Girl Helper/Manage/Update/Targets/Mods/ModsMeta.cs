using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Update.Targets.Mods
{
    class ModsMeta : ModsBase
    {
        public ModsMeta(updateInfo info) : base(info)
        {
        }

        /// <summary>
        /// Get enabled mods list infos from meta.ini of each mod
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            var infos = new Dictionary<string, string>();

            var ModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            if (ModsList != null)
                foreach (var modname in ModsList)
                {
                    var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modname);

                    var modinfo = GetIMetaInfo(ModPath);

                    if (!string.IsNullOrWhiteSpace(modinfo))
                    {
                        infos.Add(ModPath, modinfo);
                    }
                }

            return infos;
        }

        private string GetIMetaInfo(string ModPath)
        {
            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (File.Exists(metaPath))
            {
                var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");

                //updgit::BepInEx,BepInEx,BepInEx_x64::
                string UpdateInfo = Regex.Match(metaNotes, info.SourceID + "::[^:]+::").Value;

                if (!string.IsNullOrWhiteSpace(UpdateInfo) && UpdateInfo.StartsWith(info.SourceID, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    UpdateInfo = UpdateInfo.Remove(UpdateInfo.Length - 2, 2).Remove(0, info.SourceID.Length + 2);
                    //info.TargetCurrentVersion = ManageINI.GetINIValueIfExist(metaPath, "version", "General");
                    return (UpdateInfo /*+ "," + ManageINI.GetINIValueIfExist(metaPath, "version", "General")*/);
                }
            }

            return "";
        }
    }
}
