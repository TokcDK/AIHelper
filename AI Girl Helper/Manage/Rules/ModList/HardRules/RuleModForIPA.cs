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
            //exists dll in Mod\IPA or in Mod\Plugins folder but not exists loader IllusionInjector.dll or BepInEx.IPAVirtualizer.dll
            return (!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "IPA"), "*.dll", null, true)
                ||
                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "Plugins"), "*.dll", null, true)
                )
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "IPA", "Data", "Managed", "IllusionInjector.dll"))
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "BepInEx", "patchers", "BepInEx.IPAVirtualizer.dll"));
        }

        internal override string Description()
        {
            return "Mod requires IPA";
        }

        internal override bool Fix()
        {
            if (FindModWithThePath(new[] {
                "IPA" + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "IllusionInjector.dll"
                }
            , out outModName))
            {
                return true;
            }
            else
            {
                return FindModWithThePath(new[] {
                "BepInEx" + Path.DirectorySeparatorChar + "patchers" + Path.DirectorySeparatorChar + "BepInEx.IPAVirtualizer.dll"
                }
                , out outModName);
            }
        }
    }
}
