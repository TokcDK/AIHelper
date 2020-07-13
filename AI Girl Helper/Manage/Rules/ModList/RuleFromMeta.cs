using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromMeta : ModListRules
    {
        public RuleFromMeta(string ModName) : base(ModName)
        {
            ModsMustBeEnabled = new List<string>();
            ModsMustBeDisabled = new List<string>();
            ModsMustBeEnabledCandidates = new List<string>();
            ModsMustBeDisabledCandidates = new List<string>();
        }

        internal override bool Condition()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (!File.Exists(metaPath))
                return false;

            var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");

            return metaNotes.Contains("mlinfo::");
        }

        internal override string Description()
        {
            return "Rules from meta.ini of the mod";
        }

        internal override bool Fix()
        {
            ParseRulesFromMeta();
            return false;
        }

        private void ParseRulesFromMeta()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (File.Exists(metaPath))
            {
                var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");
                //var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                var mlinfo = ManageHTML.GetMLInfoTextFromHTML(metaNotes);

                if (!string.IsNullOrWhiteSpace(mlinfo))
                {
                    ParseRules(mlinfo.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        private void ParseRules(string[] rules)
        {
            foreach (var rule in rules)
            {
                if (rule.StartsWith(RulesTagREQ, StringComparison.InvariantCulture))
                {
                    ParseREQ(ModName, rule);
                }
                else if (rule.StartsWith(RulesTagINC, StringComparison.InvariantCulture))
                {
                    ParseINC(ModName, rule);
                }
            }
        }

        private void ParseINC(string modname, string rule)
        {
            var ruleData = rule.Remove(0, 4).Trim();
            if (ruleData.Contains(RulesTagOR) || ruleData.Contains(RulesTagAND))
            {
                var ruleDatas = ruleData.Split(new[] { RulesTagOR, RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var SubRule in ruleDatas)
                {
                    if (ParseINCSearchInEnabledMods(modname, SubRule))
                    {
                        return;
                    }
                }
            }
            else
            {
                ParseINCSearchInEnabledMods(modname, ruleData);
            }
        }

        private bool ParseINCSearchInEnabledMods(string modname, string ruleData)
        {
            bool IsAnyTrue;
            if (ruleData.StartsWith(RulesTagFile, StringComparison.InvariantCulture))
            {
                IsAnyTrue = ParseINCSearchFileInEnabledMods(modname, ruleData);
            }
            else
            {
                IsAnyTrue = ParseINCSearchModNameInEnabledMods(modname, ruleData);
            }
            return IsAnyTrue;
        }

        private bool ParseINCSearchModNameInEnabledMods(string modname, string ruleData)
        {
            if (EnabledModsList.Contains(ruleData))
            {
                if (!ModsMustBeDisabled.Contains(modname))
                {
                    ModsMustBeDisabled.Add(modname);
                }
                return true;
            }
            return false;
        }

        private bool ParseINCSearchFileInEnabledMods(string modname, string ruleData)
        {
            ruleData = ruleData.Remove(0, 5).TrimStart();
            if (ManageMOMods.IsFileDirExistsInDataOROverwrite(ruleData, out _))
            {
                if (!ModsMustBeDisabled.Contains(modname))
                {
                    ModsMustBeDisabled.Add(modname);
                }
                return true;
            }
            foreach (var EnabledModName in EnabledModsList)
            {
                var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), EnabledModName);
                var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                {
                    if (!ModsMustBeDisabled.Contains(modname))
                    {
                        ModsMustBeDisabled.Add(modname);
                    }
                    return true;
                }
            }
            return false;
        }

        private void ParseREQ(string modname, string rule)
        {
            var ruleData = rule.Remove(0, 4).Trim();
            var or = ruleData.Contains(RulesTagOR);
            var and = ruleData.Contains(RulesTagAND);
            if (or && and)
            {
                ParseREQORAND(ruleData, modname);
            }
            else if (or)
            {
                ParseREQOR(ruleData, modname);
            }
            else if (and)
            {
                ParseREQAND(ruleData, modname);
            }
            else
            {
                ParseREQSearchFileModNameInMods(modname, ruleData);
            }
        }

        private bool ParseREQORAND(string ruleData, string modname)
        {
            var ruleDatas = ruleData.Split(new[] { RulesTagOR, RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);

            //foreach (var SubRule in ruleDatas)
            //{

            //}
            //bool result = false;
            //MatchCollection mc = Regex.Matches(ruleData, @"(\|or\|)|(\|and\|)");
            int startIndex = 0;
            int cnt = 1;
            List<string> line = new List<string>();
            int ORindex = 0;
            int ANDindex = 0;
            bool AllIsTrue = false;
            string first = string.Empty;
            string second = string.Empty;
            while (cnt < ruleDatas.Length)
            {
                if (ORindex != -1)
                {
                    ORindex = ruleData.IndexOf(RulesTagOR, startIndex, StringComparison.InvariantCulture);
                }
                if (ANDindex != -1)
                {
                    ANDindex = ruleData.IndexOf(RulesTagAND, startIndex, StringComparison.InvariantCulture);
                }

                if (ORindex != -1 && (ORindex < ANDindex || ANDindex == -1))
                {
                    //"modsA|or|modB|and|modC"
                    if (ANDindex == -1)
                    {
                        var ORruleData = ruleData.Substring(startIndex, ORindex - startIndex);
                        if (!ParseREQOR(ORruleData, modname))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        startIndex = ORindex + 1;
                    }
                }
                else if (ANDindex != -1 && (ANDindex < ORindex || ORindex == -1))
                {
                    startIndex = ANDindex + 1;
                }
                else if (ANDindex == -1 && ORindex == -1)
                {
                    startIndex = ruleData.Length + 1;
                }
                if (cnt > 2 && cnt == ruleDatas.Length - 1)
                {
                    if (!ParseREQSearchFileModNameInMods(modname, ruleData))
                    {
                        return false;
                    }
                }
                cnt++;
            }

            AddCandidates();
            return true;
        }

        private bool ParseREQAND(string ruleData, string modname, bool addCandidates = true)
        {
            var ruleDatas = ruleData.Split(new[] { RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var SubRule in ruleDatas)
            {
                if (!ParseREQSearchFileModNameInMods(modname, SubRule, 1))
                {
                    ModsMustBeEnabledCandidates.Clear();
                    ModsMustBeDisabledCandidates.Clear();
                    return false;
                }
            }
            if (addCandidates)
            {
                AddCandidates();
            }
            return true;
        }

        private bool ParseREQOR(string ruleData, string modname, bool addCandidates = true)
        {
            var ruleDatas = ruleData.Split(new[] { RulesTagOR }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var SubRule in ruleDatas)
            {
                if (ParseREQSearchFileModNameInMods(modname, SubRule, 2))
                {
                    if (addCandidates)
                    {
                        AddCandidates();
                    }
                    return true;
                }
            }
            return false;
        }

        private void AddCandidates()
        {
            AddCandidatesToMain(ModsMustBeEnabledCandidates, ModsMustBeEnabled);
            AddCandidatesToMain(ModsMustBeDisabledCandidates, ModsMustBeDisabled);
        }

        private static void AddCandidatesToMain(List<string> candidates, List<string> parent)
        {
            if (candidates.Count > 0)
            {
                foreach (var candidate in candidates)
                {
                    if (!parent.Contains(candidate))
                    {
                        parent.Add(candidate);
                    }
                }
            }
        }

        /// <summary>
        /// Search required mod or file
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="ruleData"></param>
        /// <param name="modeANDOR">0 - none, 1 - AND, 2 - OR</param>
        /// <returns></returns>
        private bool ParseREQSearchFileModNameInMods(string modname, string ruleData, int modeANDOR = 0)
        {
            bool IsAnyTrue;
            if (ruleData.StartsWith(RulesTagFile, StringComparison.InvariantCulture))
            {
                IsAnyTrue = ParseREQSearchFileInMods(modname, ruleData, modeANDOR);
            }
            else
            {
                IsAnyTrue = ParseREQSearchModNameInMods(modname, ruleData, modeANDOR);
            }
            return IsAnyTrue;
        }

        private bool ParseREQSearchModNameInMods(string modname, string ruleData, int modeANDOR = 0)
        {
            if (!EnabledModsList.Contains(ruleData))
            {
                if (AllModsList.Contains(ruleData))
                {
                    if (modeANDOR > 0)
                    {
                        if (!ModsMustBeEnabledCandidates.Contains(ruleData))
                        {
                            ModsMustBeEnabledCandidates.Add(ruleData);
                        }
                    }
                    else
                    {
                        if (!ModsMustBeEnabled.Contains(ruleData))
                        {
                            ModsMustBeEnabled.Add(ruleData);
                        }
                    }
                    return true;
                }
                else
                {
                    if (modeANDOR > 0)
                    {
                        if (!ModsMustBeDisabledCandidates.Contains(modname))
                        {
                            ModsMustBeDisabledCandidates.Add(modname);
                        }
                    }
                    else
                    {
                        if (!ModsMustBeDisabled.Contains(modname))
                        {
                            ModsMustBeDisabled.Add(modname);
                        }
                    }
                }
            }
            return false;
        }

        private bool ParseREQSearchFileInMods(string modname, string ruleData, int modeANDOR = 0)
        {
            ruleData = ruleData.Remove(0, 5).TrimStart();
            if (ManageMOMods.IsFileDirExistsInDataOROverwrite(ruleData, out _))
            {
                return true;
            }
            foreach (var SubModName in AllModsList)
            {
                var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName);
                var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                {
                    if (!EnabledModsList.Contains(SubModName))
                    {
                        if (modeANDOR > 0)
                        {
                            if (!ModsMustBeEnabledCandidates.Contains(ruleData))
                            {
                                ModsMustBeEnabledCandidates.Add(ruleData);
                            }
                        }
                        else
                        {
                            if (!ModsMustBeEnabled.Contains(modname))
                            {
                                ModsMustBeEnabled.Add(modname);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool IsModListRule(string line)
        {
            return line.StartsWith("req:", StringComparison.InvariantCulture)
                        ||
                   line.StartsWith("inc:", StringComparison.InvariantCulture);
        }
    }
}
