using CheckForEmptyDir;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForIpa : ModListRulesBase
    {
        public RuleModForIpa(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            //exists dll in Mod\IPA or in Mod\Plugins folder but not exists loader IllusionInjector.dll or BepInEx.IPAVirtualizer.dll
            return (!Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "IPA").IsNullOrEmptyDirectory("*.dll", null, true)
                ||
                !Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "Plugins").IsNullOrEmptyDirectory("*.dll", null, true)
                )
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "IPA", "Data", "Managed", "IllusionInjector.dll"))
                && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.ModName, "BepInEx", "patchers", "BepInEx.IPAVirtualizer.dll"));
        }

        internal override string Description()
        {
            return "Mod requires IPA";
        }

        internal override bool Fix()
        {
            return FindModWithThePath(new[] {
                "IPA" + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "IllusionInjector.dll",
                "BepInEx" + Path.DirectorySeparatorChar + "patchers" + Path.DirectorySeparatorChar + "BepInEx.IPAVirtualizer.dll"
                }
            , out OutModName, 2);
        }
    }
}
