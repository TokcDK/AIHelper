using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForBepInex : HardcodedRules
    {
        public RuleModForBepInex(string ModName) : base(ModName)
        {
        }

        internal override bool Condition()
        {
            return !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName), "*.dll"
                );
        }

        internal override string Description()
        {
            return "Mod requires BepInEx";
        }

        internal override bool Fix()
        {
            return FindModPath(out outModName);
        }
    }
}
