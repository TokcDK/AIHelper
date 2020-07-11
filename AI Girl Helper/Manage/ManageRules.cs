using AIHelper.Manage.Rules.ModList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage
{
    internal class ManageRules
    {
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
            internal List<HardcodedRules> HCRulesList;
        }

        internal class ModList
        {
            ModListData modlistData;
            public ModList()
            {
                modlistData = new ModListData();
            }

            internal void ModlistFixes()
            {
                modlistData.AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
                modlistData.EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

                //Dictionary<string, string[]> rulesDict;
                var rulesExists = FillrulesDict(out Dictionary<string, string[]> rulesDict);
                var OverwritePath = ManageSettings.GetCurrentGameMOOverwritePath();

                //parse rules from meta.ini
                foreach (var ModName in modlistData.EnabledModsList)
                {
                    var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName);

                    ApplyHardCodedRules(ModName);

                    var metaPath = Path.Combine(ModPath, "meta.ini");
                    if (File.Exists(metaPath))
                    {
                        var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");
                        var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                        var req = false;
                        var inc = false;
                        if ((req = metaNotes.Contains(modlistData.RulesTagREQ)) || (inc = metaNotes.Contains("inc:")))
                        {
                            var subrules = new List<string>();
                            foreach (var line in metaNotes.SplitToLines())
                            {
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    continue;
                                }

                                var trimmedLine = line.Trim();
                                if ((req && trimmedLine.StartsWith(modlistData.RulesTagREQ, StringComparison.InvariantCulture))
                                    || (inc && trimmedLine.StartsWith(modlistData.RulesTagINC, StringComparison.InvariantCulture))
                                    )
                                {
                                    subrules.Add(trimmedLine);
                                }
                            }
                            if (subrules.Count > 0)
                            {
                                ParseRules(ModName, subrules.ToArray());
                                //rulesDict.Add(Mod, subrules.ToArray());
                            }
                        }
                    }
                }
            }

            private void ApplyHardCodedRules(string modName)
            {
                if (modlistData.HCRulesList == null)
                {
                    modlistData.HCRulesList = new List<HardcodedRules>
                    {
                         new RuleModForBepInex(modName)
                    };
                }

                foreach (var rule in modlistData.HCRulesList)
                {
                    if (rule.Condition())
                    {
                        rule.Fix();
                    }
                }
            }

            private void ParseRules(string modname, string[] rules)
            {
                foreach (var rule in rules)
                {
                    if (rule.StartsWith(modlistData.RulesTagREQ, StringComparison.InvariantCulture))
                    {
                        ParseREQ(modname, rule);
                    }
                    else if (rule.StartsWith(modlistData.RulesTagINC, StringComparison.InvariantCulture))
                    {
                        ParseINC(modname, rule);
                    }
                }
            }

            private void ParseINC(string modname, string rule)
            {
                var ruleData = rule.Remove(0, 4).Trim();
                if (ruleData.Contains(modlistData.RulesTagOR) || ruleData.Contains(modlistData.RulesTagAND))
                {
                    var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR, modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
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
                if (ruleData.StartsWith(modlistData.RulesTagFile, StringComparison.InvariantCulture))
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
                if (modlistData.EnabledModsList.Contains(ruleData))
                {
                    if (!modlistData.ModsMustBeDisabled.Contains(modname))
                    {
                        modlistData.ModsMustBeDisabled.Add(modname);
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
                    if (!modlistData.ModsMustBeDisabled.Contains(modname))
                    {
                        modlistData.ModsMustBeDisabled.Add(modname);
                    }
                    return true;
                }
                foreach (var EnabledModName in modlistData.EnabledModsList)
                {
                    var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), EnabledModName);
                    var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                    if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                    {
                        if (!modlistData.ModsMustBeDisabled.Contains(modname))
                        {
                            modlistData.ModsMustBeDisabled.Add(modname);
                        }
                        return true;
                    }
                }
                return false;
            }

            private void ParseREQ(string modname, string rule)
            {
                var ruleData = rule.Remove(0, 4).Trim();
                var or = ruleData.Contains(modlistData.RulesTagOR);
                var and = ruleData.Contains(modlistData.RulesTagAND);
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
                var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR, modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);

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
                        ORindex = ruleData.IndexOf(modlistData.RulesTagOR, startIndex, StringComparison.InvariantCulture);
                    }
                    if (ANDindex != -1)
                    {
                        ANDindex = ruleData.IndexOf(modlistData.RulesTagAND, startIndex, StringComparison.InvariantCulture);
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
                var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var SubRule in ruleDatas)
                {
                    if (!ParseREQSearchFileModNameInMods(modname, SubRule, 1))
                    {
                        modlistData.ModsMustBeEnabledCandidates.Clear();
                        modlistData.ModsMustBeDisabledCandidates.Clear();
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
                var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR }, StringSplitOptions.RemoveEmptyEntries);
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
                AddCandidatesToMain(modlistData.ModsMustBeEnabledCandidates, modlistData.ModsMustBeEnabled);
                AddCandidatesToMain(modlistData.ModsMustBeDisabledCandidates, modlistData.ModsMustBeDisabled);
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
                if (ruleData.StartsWith(modlistData.RulesTagFile, StringComparison.InvariantCulture))
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
                if (!modlistData.EnabledModsList.Contains(ruleData))
                {
                    if (modlistData.AllModsList.Contains(ruleData))
                    {
                        if (modeANDOR > 0)
                        {
                            if (!modlistData.ModsMustBeEnabledCandidates.Contains(ruleData))
                            {
                                modlistData.ModsMustBeEnabledCandidates.Add(ruleData);
                            }
                        }
                        else
                        {
                            if (!modlistData.ModsMustBeEnabled.Contains(ruleData))
                            {
                                modlistData.ModsMustBeEnabled.Add(ruleData);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        if (modeANDOR > 0)
                        {
                            if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                            {
                                modlistData.ModsMustBeDisabledCandidates.Add(modname);
                            }
                        }
                        else
                        {
                            if (!modlistData.ModsMustBeDisabled.Contains(modname))
                            {
                                modlistData.ModsMustBeDisabled.Add(modname);
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
                foreach (var SubModName in modlistData.AllModsList)
                {
                    var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName);
                    var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                    if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                    {
                        if (!modlistData.EnabledModsList.Contains(SubModName))
                        {
                            if (modeANDOR > 0)
                            {
                                if (!modlistData.ModsMustBeEnabledCandidates.Contains(ruleData))
                                {
                                    modlistData.ModsMustBeEnabledCandidates.Add(ruleData);
                                }
                            }
                            else
                            {
                                if (!modlistData.ModsMustBeEnabled.Contains(modname))
                                {
                                    modlistData.ModsMustBeEnabled.Add(modname);
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            private static bool FillrulesDict(out Dictionary<string, string[]> rulesDict)
            {
                var modlistRules = Path.Combine(ManageSettings.GetAppResDir(), "rules", ManageSettings.GetCurrentGameFolderName(), "modlist.txt");
                if (File.Exists(modlistRules))
                {
                    rulesDict = null;
                    return false;
                }

                rulesDict = new Dictionary<string, string[]>();
                string ruleName = string.Empty;
                List<string> ruleConditions = new List<string>();
                using (var sr = new StreamReader(modlistRules))
                {
                    bool ruleReading = false;
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith(";", StringComparison.InvariantCulture))
                        {
                            continue;
                        }

                        if (!ruleReading)
                        {
                            ruleName = line.Trim();
                            ruleReading = true;
                        }
                        else if (IsModListRule(line))
                        {
                            ruleConditions.Add(line.Trim());
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                            {
                                rulesDict.Add(ruleName, ruleConditions.ToArray());
                            }

                            ruleName = string.Empty;
                            ruleConditions.Clear();
                            ruleReading = false;
                        }
                    }
                }

                return rulesDict.Count > 0;
            }

            private static bool IsModListRule(string line)
            {
                return line.StartsWith("req:", StringComparison.InvariantCulture)
                            ||
                       line.StartsWith("inc:", StringComparison.InvariantCulture);
            }
        }
    }
}
