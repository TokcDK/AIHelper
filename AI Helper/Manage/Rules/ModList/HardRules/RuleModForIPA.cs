using CheckForEmptyDir;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForIpa : ModListRulesBase
    {
        public RuleModForIpa(ModListRulesData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            //exists dll in Mod\IPA or in Mod\Plugins folder but not exists loader IllusionInjector.dll or BepInEx.IPAVirtualizer.dll
            return (!Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "IPA").IsNullOrEmptyDirectory("*.dll", null, true)
                ||
                !Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "Plugins").IsNullOrEmptyDirectory("*.dll", null, true)
                )
                && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "IPA", "Data", "Managed", "IllusionInjector.dll"))
                && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "patchers", "BepInEx.IPAVirtualizer.dll"));
        }

        internal override string Description()
        {
            return "Mod requires IPA";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("IPA" + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "Managed" + Path.DirectorySeparatorChar + "IllusionInjector.dll", out _)
                || FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "patchers" + Path.DirectorySeparatorChar + "BepInEx.IPAVirtualizer.dll", out _);
        }
    }
}
