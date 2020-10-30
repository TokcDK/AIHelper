using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForOverlaysPlugin : ModListRulesBase
    {
        public RuleModForOverlaysPlugin(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return modlistData.GamePrefix.Length > 0 && (
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "Overlays"), "*.png", null, true
                )
                    )
                    && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "plugins", modlistData.GamePrefix + "_OverlayMods.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Overlays plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + modlistData.GamePrefix + "_OverlayMods.dll", out outModName);
        }
    }
}
