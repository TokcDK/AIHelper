using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Functions.ModlistFixes.Data.Enums;
using AIHelper.Manage.Functions.ModlistFixes.Tags.SearchTarget;
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
            if (!regex.Success) return;

            var rules = regex.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(rules)) return;

            var rulesData = new ModlistFixesPrefixesData(rules);
            if (rulesData.PrefixesDataList.Count == 0) return;

            RulesDataList.Add(rulesData);
        }

        internal List<ModlistFixesPrefixesData> RulesDataList { get; } = new List<ModlistFixesPrefixesData>();
    }
    internal class ModlistFixesPrefixesData
    {
        public ModlistFixesPrefixesData(string rules)
        {
            var prefixes = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<ISearchTypeTag>();

            var rulesList = rules.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < rulesList.Length; n++)
            {
                var rule = rulesList[n];

                var prefixMatches = Regex.Matches(rule, "[0-9a-z]+:", RegexOptions.IgnoreCase);
                if (prefixMatches.Count == 0) continue;

                var prefixMatchesCount = prefixMatches.Count;
                for (int i = prefixMatchesCount - 1; i >= 0; i--)
                {
                    var match = prefixMatches[i];

                    foreach (var prefix in prefixes)
                    {
                        if (!string.Equals(prefix.Tag, match.Value)) continue;

                        var s = rule.Substring(match.Index + match.Length);
                        var prefixdata = new ModlistFixesPrefixData(s, prefix);
                        if (prefixdata.SplittersDataList != null) PrefixesDataList.Add(prefixdata);

                        rule = rule.Remove(match.Index);
                    }
                }
            }
        }
        internal List<ModlistFixesPrefixData> PrefixesDataList { get; } = new List<ModlistFixesPrefixData>();
    }
    internal class ModlistFixesPrefixData
    {
        public ModlistFixesPrefixData(string rulesData, ISearchTypeTag prefixType)
        {
            PrefixType = prefixType;

            var sData = GetSplitterData(rulesData);
            if (sData == null) return;

            SplittersDataList = sData;

            //var splittersMatches = Regex.Matches(rulesData, @"\|[0-9a-z-A-Z]+\|", RegexOptions.IgnoreCase);
            //var splitterMatchesStringList = new List<string>();
            //var max = splittersMatches.Count - 1;
            //var maxlength = rulesData.Length;
            //var startIndex = 0;
            //ModlistFixesRuleTargetData splitterTargetLeft = null;
            //for (int i = 0; i <= max; i++)
            //{
            //    var splitterMatch = splittersMatches[i];

            //    // search tag for the match
            //    var splitterTag = splitters.FirstOrDefault(t => string.Equals(t.Tag, splitterMatch.Value, System.StringComparison.InvariantCultureIgnoreCase));
            //    if (splitterTag == default)
            //    {
            //        startIndex = splitterMatch.Index + splitterMatch.Length;
            //        continue;
            //    }

            //    var splitterData = new ModlistFixesRulesSplitterData(splitterTag);

            //    if (splitterTargetLeft == null)
            //    {
            //        var splitterTargetLeftS = rulesData.Substring(startIndex, splitterMatch.Index);
            //        splitterTargetLeft = new ModlistFixesRuleTargetData(splitterTargetLeftS);
            //    }
            //    splitterData.Targets.Add(splitterTargetLeft);

            //    var length = (i == max ? maxlength : splittersMatches[i + 1].Index) - splitterMatch.Index;
            //    var righttargetStartIndex = splitterMatch.Index + splitterMatch.Length;
            //    var splitterTargetRightS = rulesData.Substring(righttargetStartIndex, length);
            //    var splitterTargetRight = new ModlistFixesRuleTargetData(splitterTargetRightS);
            //    splitterData.Targets.Add(splitterTargetRight);

            //    SplittersDataList.Add(splitterData);

            //    startIndex = righttargetStartIndex;
            //}
            //splitterMatchesStringList.Reverse();

            //foreach (var splitter in splitters.OrderBy(s => s.Order))
            //{
            //    int splitterIndex = -1;
            //    int startIndex = 0;
            //    while ((splitterIndex = rulesData.IndexOf(splitter.Tag, startIndex)) != -1)
            //    {
            //        var nexIndex =
            //    }
            //}
        }

        private ModlistFixesRulesSplitterData GetSplitterData(string rulesData)
        {
            var splitters = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<ISplitterTag>().ToArray();
            if (splitters.Length == 0) return null;

            ModlistFixesRulesSplitterData parentSplitterData = null;
            foreach (var splitter in splitters.OrderBy(s => s.Order))
            {
                var dataSplitted = rulesData.Split(new[] { splitter.Tag }, System.StringSplitOptions.RemoveEmptyEntries);

                bool mergingMode = false;
                object part = null;
                for (int i = 0; i < dataSplitted.Length; i++)
                {
                    string target = dataSplitted[i];

                    if (mergingMode || i >= 1)
                    {
                        mergingMode = true;

                        // init splitter data
                        var sData = new ModlistFixesRulesSplitterData(splitter);

                        // add targets
                        var tData = new ModlistFixesRuleTargetData(part as string);
                        sData.Targets.Add(tData);
                        tData = new ModlistFixesRuleTargetData(target);
                        sData.Targets.Add(tData);

                        // set as parent
                        if (parentSplitterData == null) parentSplitterData = sData;
                        else parentSplitterData.Childs.Add(sData);
                    }
                    else
                    {
                        part = target;
                    }
                }
            }

            return parentSplitterData;
        }

        internal ISearchTypeTag PrefixType { get; }
        internal ModlistFixesRulesSplitterData SplittersDataList { get; }
    }
    internal class ModlistFixesRulesSplitterData
    {
        public ModlistFixesRulesSplitterData(ISplitterTag splitter)
        {
            Splitter = splitter;
        }

        internal ISplitterTag Splitter { get; }
        internal List<ModlistFixesRuleTargetData> Targets { get; } = new List<ModlistFixesRuleTargetData>();
        internal List<ModlistFixesRulesSplitterData> Childs { get; } = new List<ModlistFixesRulesSplitterData>();
    }

    internal class ModlistFixesRuleTargetData
    {
        public ModlistFixesRuleTargetData(string s)
        {
            var targets = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<ISearchTargetPrefixTag>();

            foreach(var t in targets)
            {
                if (!s.StartsWith(t.Tag)) continue;

                Type = t;
                Target = s.Substring(t.Tag.Length);

                break;
            }

            if (Type == default) Target = s; // set input string sd default modname value
        }

        internal string Target { get; }
        internal ISearchTargetPrefixTag Type { get; }
    }
}
