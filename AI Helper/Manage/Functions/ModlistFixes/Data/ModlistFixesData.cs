using System.Collections.Generic;
using System.Text.RegularExpressions;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Functions.ModlistFixes.Data.Enums;
using AIHelper.Manage.Functions.ModlistFixes.Tags.SearchType;
using AIHelper.Manage.Functions.ModlistFixes.Tags.Splitters;

namespace AIHelper.Manage.Functions.ModlistFixes.Data
{
    internal class ModlistFixesData
    {
        public ModlistFixesData(ModData mod)
        {
            Mod = mod;
        }

        /// <summary>
        /// Currently parsing mod data
        /// </summary>
        internal ModData Mod { get; }
        internal List<string> Rules { get; }


        internal List<string> Messages { get; } = new List<string>();
    }
    internal class ModlistFixesRulesData
    {
        const string _rulesMarkerStartsWith = "::mlinfo::";
        const string _rulesMarkerEndsWith = "::";

        public ModlistFixesRulesData(string rulesString)
        {
            if (string.IsNullOrWhiteSpace(rulesString)) return;
            var regex = Regex.Match(rulesString, _rulesMarkerStartsWith + "(.+)" + _rulesMarkerEndsWith);
            if(!regex.Success) return;

            var rules = regex.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(rules)) return;

            var rulesData = new ModlistFixesPrefixData(rules);
            if (rulesData.SplittersDataList.Count == 0) return;

            RulesDataList.Add(rulesData);
        }

        internal List<ModlistFixesPrefixData> RulesDataList { get; } = new List<ModlistFixesPrefixData>();
    }
    internal class ModlistFixesPrefixData
    {
        public ModlistFixesPrefixData(string rules)
        {
            var rulesList = rules.Split(new[] {"\r\n","\r","\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(var rule in rulesList)
            {

            }
        }

        internal ISearchTypeTag PrefixType { get; }
        internal List<ModlistFixesRulesSplitterData> SplittersDataList { get; } = new List<ModlistFixesRulesSplitterData>();
    }
    internal class ModlistFixesRulesSplitterData
    {
        public ModlistFixesRulesSplitterData(ISplitterTag splitter)
        {
            Splitter = splitter;
        }

        internal ISplitterTag Splitter { get; }
        internal List<ModlistFixesRuleTargetData> Targets { get; } = new List<ModlistFixesRuleTargetData>();
    }

    internal class ModlistFixesRuleTargetData
    {
        public ModlistFixesRuleTargetData(string target, ModlistFixesRuleTargetType type = ModlistFixesRuleTargetType.DirName)
        {
            Target = target;
            Type = type;
        }

        internal string Target { get; }
        internal ModlistFixesRuleTargetType Type { get; }
    }
}
