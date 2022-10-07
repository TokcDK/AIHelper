using AIHelper.Data.Modlist;
using AIHelper.SharedData;
using System;
using System.Collections.Generic;

namespace AIHelper.Manage.Rules.ModList
{
    internal class ModListRulesData
    {
        internal ModData[] AllModNamesList;
        internal ModData[] EnabledModNamesList;
        internal Dictionary<string, string> ModsMustBeEnabled = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeDisabled = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeEnabledCandidates = new Dictionary<string, string>();
        internal Dictionary<string, string> ModsMustBeDisabledCandidates = new Dictionary<string, string>();
        internal string RulesTagOr = "|or|";
        internal string RulesTagAnd = "|and|";
        internal string RulesTagFile = "file:";
        internal string RulesTagReq = "req:";
        internal string RulesTagInc = "inc:";
        internal List<ModListRulesBase> RulesList;
        internal List<string> Report = new List<string>();
        internal ModData Mod;
        internal string GamePrefix;
        internal Dictionary<string, string[]> RulesDict;
        internal Dictionary<string, string[]> RulesDictOverall;



        public ModListRulesData()
        {
            GamePrefix = ManageSettings.Games.Game.GameAbbreviation;
            RulesList = GetListOfSubClasses.Inherited.GetListOfinheritedSubClasses<ModListRulesBase>(this);
        }

        /// <summary>
        /// Comment tags: ";"
        /// </summary>
        internal static string[] CommentTag = new[] { ";" };
        internal bool KPlugEnabled;

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

        internal static string GetDataWithNoComments(string line)
        {
            return line.Split(CommentTag, StringSplitOptions.None)[0].TrimEnd();
        }
    }
}
