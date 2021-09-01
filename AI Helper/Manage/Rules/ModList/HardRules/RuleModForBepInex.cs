using CheckForEmptyDir;
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
            return (!Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), ModlistData.ModName, "BepInEx", "plugins").IsNullOrEmptyDirectory("*.dll", null, true
                ) || !Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), ModlistData.ModName, "BepInEx", "patchers").IsNullOrEmptyDirectory("*.dll", null, true
                )) && !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), ModlistData.ModName, "BepInEx", "core", "BepInEx.dll"));
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
