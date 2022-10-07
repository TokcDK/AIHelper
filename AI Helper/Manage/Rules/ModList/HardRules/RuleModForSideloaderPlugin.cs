using CheckForEmptyDir;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForSideloaderPlugin : ModListRulesBase
    {
        public RuleModForSideloaderPlugin(ModListRulesData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (
                !Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "mods").IsNullOrEmptyDirectory("*.zip", null, true
                )
                ||
                !Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "mods").IsNullOrEmptyDirectory("*.zipmod", null, true
                ))
                && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "plugins", ModlistData.GamePrefix + "_BepisPlugins", "Sideloader.dll"))
                && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "plugins", ModlistData.GamePrefix + "_BepisPlugins", ModlistData.GamePrefix + "_Sideloader.dll"));
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
            , out _);
        }
    }
}
