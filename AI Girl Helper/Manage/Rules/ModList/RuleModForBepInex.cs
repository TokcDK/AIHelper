using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForBepInex : ModListRules
    {
        public RuleModForBepInex(string ModName) : base(ModName)
        {
        }

        internal override bool Condition()
        {
            return (!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName, "BepInEx", "plugins"), "*.dll"
                ) || !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName, "BepInEx", "patchers"), "*.dll"
                )) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName, "BepInEx", "core", "BepInEx.dll"));
        }

        internal override string Description()
        {
            return "Mod requires BepInEx";
        }

        internal override bool Fix()
        {
            return FindModPath("BepInEx" + Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar + "BepInEx.dll", out outModName);
        }
    }
}
