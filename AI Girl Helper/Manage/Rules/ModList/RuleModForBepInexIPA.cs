using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForBepInexIPA : ModListRules
    {
        public RuleModForBepInexIPA(string ModName) : base(ModName)
        {
        }

        internal override bool Condition()
        {
            return !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName, "BepInEx", "IPA"), "*.dll"
                ) &&
                 !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName, "BepInEx", "patchers", "BepInEx.IPAVirtualizer.dll"));
        }

        internal override string Description()
        {
            return "Mod requires BepInEx IPA loader";
        }

        internal override bool Fix()
        {
            //List<string> modNamesToActivate = new List<string>();
            if (FindModPath("BepInEx" + Path.DirectorySeparatorChar + "patchers" + Path.DirectorySeparatorChar + "BepInEx.IPAVirtualizer.dll", out outModName))
            {
                if (!string.IsNullOrWhiteSpace(outModName) && outModName != "data" && outModName != "overwrite")
                {
                    ManageMO.ActivateInsertModIfPossible(outModName);
                    Result = "Was enabled required mod" + " \"" + outModName + "\"";
                    return true;
                }
            }

            return false;
        }
    }
}
