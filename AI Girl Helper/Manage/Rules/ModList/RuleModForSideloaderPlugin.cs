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
                )) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "plugins", "KK_BepisPlugins", "Sideloader.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Sideloader plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + "KK_BepisPlugins" + Path.DirectorySeparatorChar + "Sideloader.dll", out outModName);
        }
    }
}
