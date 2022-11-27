using CheckForEmptyDir;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForBepInex : ModListRulesBase
    {
        public RuleModForBepInex(ModListRulesData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return (!Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "plugins").IsNullOrEmptyDirectory("*.dll", null, true
                ) || !Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "patchers").IsNullOrEmptyDirectory("*.dll", null, true
                )) && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name, "BepInEx", "core", "BepInEx.dll"));
        }

        internal override string Description()
        {
            return "Mod requires BepInEx";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar + "BepInEx.dll", out _);
        }
    }
}
