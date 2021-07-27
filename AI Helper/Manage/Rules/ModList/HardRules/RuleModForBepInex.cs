using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForBepInex : ModListRulesBase
    {
        public RuleModForBepInex(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "plugins"), "*.dll", null, true
                ) || !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "patchers"), "*.dll", null, true
                )) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "core", "BepInEx.dll"));
        }

        internal override string Description()
        {
            return "Mod requires BepInEx";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar + "BepInEx.dll", out OutModName);
        }
    }
}
