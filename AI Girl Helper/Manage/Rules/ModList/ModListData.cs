using System;
using System.Collections.Generic;
using System.Linq;

namespace AIHelper.Manage.Rules.ModList
{
    internal class ModListData
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
        internal string RulesTagREQ = "req:";
        internal string RulesTagINC = "inc:";
        internal List<ModListRules> HardCodedRulesList;
        internal List<string> Report = new List<string>();
        internal string ModName;
        internal Dictionary<string, string[]> rulesDict;

        public ModListData()
        {
            HardCodedRulesList = GetListOfHardcodedRules(this);
        }

        /// <summary>
        /// Get all inherited classes of an abstract class
        /// Get only HardCoded Rules
        /// </summary>
        /// <returns></returns>
        internal static List<ModListRules> GetListOfHardcodedRules(ModListData modlistData)
        {
            //https://stackoverflow.com/a/5411981
            //Get all inherited classes of an abstract class
            IEnumerable<ModListRules> SubclassesOfModListRules = typeof(ModListRules)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ModListRules)) && !t.IsAbstract)
            .Select(t => (ModListRules)Activator.CreateInstance(t, modlistData));

            return (from ModListRules SubClass in SubclassesOfModListRules
                    //where (SubClass.IsHardRule)
                    select SubClass).ToList();
        }
    }

    internal class ModlistRulesTags
    {
        internal class Main: ModlistRulesTags
        {
            internal string REQ = "req:";
            internal string INC = "inc:";
        }

        internal class Sub : ModlistRulesTags
        {
            internal string File = "file:";
            internal string OR = "|or|";
            internal string AND = "|and|";
        }
    }
}
