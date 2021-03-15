using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForkPlug : ModListRulesBase
    {
        public RuleModForkPlug(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, "Plugins", "kPlug.dll"))
                ;
        }

        internal override string Description()
        {
            return "Tweaks for kPlug";
        }

        internal override bool Fix()
        {
            return modlistData.kPlugEnabled = true;
        }
    }
}
