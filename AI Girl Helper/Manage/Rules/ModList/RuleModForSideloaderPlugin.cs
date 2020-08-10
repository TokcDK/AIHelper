using System;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForSideloaderPlugin : ModListRules
    {
        public RuleModForSideloaderPlugin(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "mods"), "*.zip", null, true
                )
                ||
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "mods"), "*.zipmod", null, true
                )) 
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "plugins", modlistData.GamePrefix + "_BepisPlugins", "Sideloader.dll"))
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "plugins", modlistData.GamePrefix + "_BepisPlugins", modlistData.GamePrefix + "_Sideloader.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Sideloader plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath(
                new[] { 
                    "BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + modlistData.GamePrefix + "_BepisPlugins" + Path.DirectorySeparatorChar + "Sideloader.dll"
                  , "BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + modlistData.GamePrefix + "_BepisPlugins" + Path.DirectorySeparatorChar + modlistData.GamePrefix + "_Sideloader.dll" 
                      }
            , out outModName);
        }
    }
}
