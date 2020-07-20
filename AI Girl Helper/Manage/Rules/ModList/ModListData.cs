using System;
using System.Collections.Generic;

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
        internal List<ModListRules> RulesList;
        internal List<string> Report = new List<string>();
        internal string ModName;
        internal Dictionary<string, string[]> rulesDict;
        internal Dictionary<string, string[]> rulesDictOverall;

        public ModListData()
        {
            RulesList = GetListOfSubClasses<ModListRules>(this);
        }

        ///// <summary>
        ///// Get all inherited classes of an abstract class (requires linq)
        ///// </summary>
        ///// <returns></returns>
        //internal static List<ModListRules> GetListOfRulesLinq(ModListData modlistData)
        //{
        //    //https://stackoverflow.com/a/5411981
        //    //Get all inherited classes of an abstract class
        //    IEnumerable<ModListRules> SubclassesOfModListRules = typeof(ModListRules)
        //    .Assembly.GetTypes()
        //    .Where(t => t.IsSubclassOf(typeof(ModListRules)) && !t.IsAbstract)
        //    .Select(t => (ModListRules)Activator.CreateInstance(t, modlistData));

        //    return (from ModListRules SubClass in SubclassesOfModListRules
        //                //where (SubClass.IsHardRule)
        //            select SubClass).ToList();
        //}

        /// <summary>
        /// Get all inherited classes of an abstract class
        /// non linq version of https://stackoverflow.com/a/5411981
        /// </summary>
        /// <typeparam name="T">type of subclasses</typeparam>
        /// <param name="parameter">parameter required for subclass(remove if not need)</param>
        /// <returns>List of subclasses of abstract class</returns>
        internal static List<T> GetListOfSubClasses<T>(ModListData parameter)
        {
            var ListOfSubClasses = new List<T>();
            foreach (var ClassType in typeof(T).Assembly.GetTypes())
            {
                if (ClassType.IsSubclassOf(typeof(T)) && !ClassType.IsAbstract)
                {
                    ListOfSubClasses.Add((T)Activator.CreateInstance(ClassType, parameter));
                }
            }

            return ListOfSubClasses;
        }
    }
}
