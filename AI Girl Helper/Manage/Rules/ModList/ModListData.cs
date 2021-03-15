using System;
using System.Collections.Generic;

namespace AIHelper.Manage.Rules.ModList
{
    internal class ModListData
    {
        internal string[] AllModsList;
        internal string[] EnabledModsList;
        internal Dictionary<string, string> ModsMustBeEnabled = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeDisabled = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeEnabledCandidates = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeDisabledCandidates = new Dictionary<string, string>();
        internal string RulesTagOR = "|or|";
        internal string RulesTagAND = "|and|";
        internal string RulesTagFile = "file:";
        internal string RulesTagREQ = "req:";
        internal string RulesTagINC = "inc:";
        internal List<ModListRulesBase> RulesList;
        internal List<string> Report = new List<string>();
        internal string ModName;
        internal string GamePrefix;
        internal Dictionary<string, string[]> rulesDict;
        internal Dictionary<string, string[]> rulesDictOverall;



        public ModListData()
        {
            GamePrefix = ManageSettings.GetListOfExistsGames()[ManageSettings.GetCurrentGameIndex()].GetGamePrefix();
            RulesList = GetListOfSubClasses<ModListRulesBase>(this);
        }

        /// <summary>
        /// Comment tags: ";"
        /// </summary>
        internal static string[] CommentTag = new[] { ";" };
        internal bool kPlugEnabled;

        /// <summary>
        /// ";" - commentary
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static bool IsComment(string line)
        {
            foreach (var comment in CommentTag)
            {
                if (line.TrimStart().StartsWith(comment, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }

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

        internal static string GetDataWithNoComments(string line)
        {
            return line.Split(CommentTag, StringSplitOptions.None)[0].TrimEnd();
        }
    }
}
