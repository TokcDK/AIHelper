using System;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForOverlaysPlugin : ModListRules
    {
        public RuleModForOverlaysPlugin(ModListData modlistData) : base(modlistData)
        {
            CurrentGamePrefix = GetGamePrefix();            
        }

        private static string GetGamePrefix()
        {
            if (ManageSettings.GetCurrentGameEXEName() == "Koikatu")
            {
                return "KK";
            }
            else if (ManageSettings.GetCurrentGameEXEName() == "AI-Syoujyo")
            {
                return "AI";
            }
            return string.Empty;
        }

        string CurrentGamePrefix = string.Empty;
        internal override bool Condition()
        {
            return CurrentGamePrefix.Length > 0 && (
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "Overlays"), "*.png", null, true
                )
                    )
                    && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "plugins", CurrentGamePrefix + "_OverlayMods.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Overlays plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + CurrentGamePrefix + "_OverlayMods.dll", out outModName);
        }
    }
}
