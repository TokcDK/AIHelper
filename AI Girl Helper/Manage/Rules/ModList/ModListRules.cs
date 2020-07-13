using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Rules.ModList
{
    internal abstract class ModListRules
    {
        protected string[] AllModsList;
        protected string[] EnabledModsList;

        protected string RulesTagOR = "|or|";
        protected string RulesTagAND = "|and|";
        protected string RulesTagFile = "file:";
        protected string RulesTagREQ = "req";
        protected string RulesTagINC = "inc:";

        protected List<string> ModsMustBeEnabled;
        protected List<string> ModsMustBeDisabled;
        protected List<string> ModsMustBeEnabledCandidates;
        protected List<string> ModsMustBeDisabledCandidates;

        protected List<ModListRules> HCRulesList;

        protected string ModName;
        internal string outModName;
        protected ModListRules(string ModName)
        {
            this.ModName = ModName;
        }

        internal abstract bool Condition();
        internal abstract bool Fix();
        internal abstract string Description();

        internal string Result = string.Empty;

        protected bool FindModPath(string inSubPath, out string FoundModName)
        {
            if (ManageMOMods.IsFileDirExistsInDataOROverwrite(inSubPath, out string Source))
            {
                FoundModName = Source;
                return true;
            }

            AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
            EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            FoundModName = string.Empty;
            foreach (var SubModName in AllModsList)
            {
                if (SubModName != ModName && File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName, inSubPath)))
                {
                    FoundModName = SubModName;
                    if(!EnabledModsList.Contains(FoundModName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
