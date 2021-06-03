﻿using System;
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
            RulesList = GetListOfSubClasses.Inherited.GetListOfinheritedSubClasses<ModListRulesBase>(this);
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

        internal static string GetDataWithNoComments(string line)
        {
            return line.Split(CommentTag, StringSplitOptions.None)[0].TrimEnd();
        }
    }
}
