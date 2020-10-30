using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForIPA : ModListRulesBase
    {
        public RuleModForIPA(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "IPA"), "*.dll", null, true)
                ||
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "Plugins"), "*.dll", null, true)
                ) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "IPA", "Data", "Managed", "IllusionInjector.dll"));
        }

        internal override string Description()
        {
            return "Mod requires IPA";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("IPA" + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "IllusionInjector.dll", out outModName);
        }
    }
}
