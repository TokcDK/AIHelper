using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal abstract class HardcodedRules
    {
        ModListData modlistData;
        protected string ModName;
        internal string outModName;
        protected HardcodedRules(string ModName)
        {
            this.ModName = ModName;
            modlistData = new ModListData();
        }

        internal abstract bool Condition();
        internal abstract bool Fix();
        internal abstract string Description();

        protected bool FindModPath(out string FoundModName)
        {
            if (ManageMOMods.IsFileDirExistsInDataOROverwrite("BepInEx" + Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar + "BepInEx.dll", out string Source))
            {
                FoundModName = Source;
                return true;
            }

            modlistData.AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
            modlistData.EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            foreach (var SubModName in modlistData.AllModsList)
            {
                if (SubModName != ModName && File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName, "BepInEx", "core", "BepInEx.dll")))
                {
                    FoundModName = SubModName;
                    return true;
                }
            }
            FoundModName = null;
            return false;
        }
    }

    class ModListData
    {
        internal string[] AllModsList;
        internal string[] EnabledModsList;
        internal List<string> ModsMustBeEnabled = new List<string>();
        internal List<string> ModsMustBeDisabled = new List<string>();
        internal List<string> ModsMustBeEnabledCandidates = new List<string>();
        internal List<string> ModsMustBeDisabledCandidates = new List<string>();
        internal string RulesTagOR = "|or|";
        internal string RulesTagAND = "|and|";
        internal string RulesTagFile = "file:";
        internal string RulesTagREQ = "req";
        internal string RulesTagINC = "inc:";
    }
}
