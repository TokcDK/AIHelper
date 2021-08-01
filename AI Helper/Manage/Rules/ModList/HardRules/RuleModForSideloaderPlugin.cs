using CheckForEmptyDir;
using System;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForSideloaderPlugin : ModListRulesBase
    {
        public RuleModForSideloaderPlugin(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (
                !Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "mods").IsNullOrEmptyDirectory("*.zip", null, true
                )
                ||
                !Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "mods").IsNullOrEmptyDirectory("*.zipmod", null, true
                )) 
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "plugins", ModlistData.GamePrefix + "_BepisPlugins", "Sideloader.dll"))
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "plugins", ModlistData.GamePrefix + "_BepisPlugins", ModlistData.GamePrefix + "_Sideloader.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Sideloader plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath(
                new[] { 
                    "BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + ModlistData.GamePrefix + "_BepisPlugins" + Path.DirectorySeparatorChar + "Sideloader.dll"
                  , "BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + ModlistData.GamePrefix + "_BepisPlugins" + Path.DirectorySeparatorChar + ModlistData.GamePrefix + "_Sideloader.dll" 
                      }
            , out OutModName);
        }
    }
}
