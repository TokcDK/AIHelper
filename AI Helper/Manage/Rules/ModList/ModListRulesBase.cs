using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Rules.ModList
{
    internal abstract class ModListRulesBase
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();

        protected ModListData ModlistData;

        internal string OutModName;
        protected ModListRulesBase(ModListData modlistData)
        {
            this.ModlistData = modlistData;
        }

        internal abstract bool Condition();
        internal abstract bool Fix();
        internal abstract string Description();
        internal virtual bool IsHardRule { get => true; }

        internal string Result = string.Empty;

        /// <summary>
        /// True if mod with the inSubPath will be found
        /// </summary>
        /// <param name="inSubPath">input path for search</param>
        /// <param name="foundModName">mod name of found mod</param>
        /// <param name="modeAndor">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string inSubPath, out string foundModName, int modeAndor = 0, bool dontAddCandidate = false)
        {
            return FindModWithThePath(new[] { inSubPath }, out foundModName, modeAndor, dontAddCandidate);
        }

        /// <summary>
        /// True if mod with the inSubPath will be found
        /// </summary>
        /// <param name="inSubPath">input path for search</param>
        /// <param name="foundModName">mod name of found mod</param>
        /// <param name="modeAndor">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string[] inSubPath, out string foundModName, int modeAndor = 0, bool dontAddCandidate = false)
        {
            RemoveRulesTagFile(ref inSubPath);

            if (File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.ModName) + Path.DirectorySeparatorChar + inSubPath))
            {
                foundModName = ModlistData.ModName;
                return true;
            }

            if (IsExistInDataOrOverwrite(inSubPath, out foundModName))
            {
                return true;
            }

            //AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
            //EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            foundModName = string.Empty;
            HashSet<string> alreadyChecked = new HashSet<string>();
            foreach (var subModName in ModlistData.AllModNamesList)
            {
                ///add modname to already checked
                if (!alreadyChecked.Contains(subModName))
                {
                    alreadyChecked.Add(subModName);
                }

                for (int i = 0; i < inSubPath.Length; i++)
                {
                    var filePath = Path.Combine(ManageSettings.CurrentGameModsDirPath, subModName) + Path.DirectorySeparatorChar + inSubPath[i];
                    if (subModName != ModlistData.ModName && File.Exists(filePath))
                    {
                        foundModName = subModName;

                        //when mod enabled return true and not add
                        if (ModlistData.EnabledModNamesList.Contains(foundModName))
                        {
                            return true;
                        }

                        ///Check if path not exists in rest of Enabled mods
                        if (IsRestOfEnabledModsContainsSameFile(alreadyChecked, inSubPath[i]))
                        {
                            return true;
                        }

                        //else if mod not enabled then add it for activation
                        if (!dontAddCandidate)
                        {
                            if (!ModlistData.ModsMustBeEnabledCandidates.ContainsKey(foundModName))
                            {
                                ModlistData.ModsMustBeEnabledCandidates.Add(foundModName, "req:" + subModName);
                            }

                            //if (modeANDOR > 0)
                            //{
                            //    if (!modlistData.ModsMustBeEnabledCandidates.ContainsKey(FoundModName))
                            //    {
                            //        modlistData.ModsMustBeEnabledCandidates.Add(FoundModName, "req:" + SubModName);
                            //    }
                            //}
                            //else
                            //{
                            //    if (!modlistData.ModsMustBeEnabledCandidates.ContainsKey(FoundModName))
                            //    {
                            //        modlistData.ModsMustBeEnabledCandidates.Add(FoundModName, "req:" + SubModName);
                            //    }
                            //    //if (!modlistData.ModsMustBeEnabled.Contains(FoundModName))
                            //    //{
                            //    //    modlistData.ModsMustBeEnabled.Add(FoundModName);
                            //    //}
                            //}
                        }

                        //if (!ModsMustBeEnabled.Contains(FoundModName))
                        //{
                        //    ModsMustBeEnabled.Add(FoundModName);
                        //}
                        return true;
                    }
                }
            }
            if (!ModlistData.ModsMustBeDisabled.ContainsKey(ModlistData.ModName))
            {
                ModlistData.ModsMustBeDisabled.Add(ModlistData.ModName, "req:" + string.Join(",", inSubPath));
            }

            return false;
        }

        private static bool IsExistInDataOrOverwrite(string[] inSubPath, out string foundModName)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (ManageModOrganizer.IsFileDirExistsInDataOrOverwrite(inSubPath[i], out string source))
                {
                    foundModName = source;
                    return true;
                }
            }
            foundModName = string.Empty;
            return false;
        }

        private void RemoveRulesTagFile(ref string[] inSubPath)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (inSubPath[i].TrimStart().StartsWith(ModlistData.RulesTagFile, System.StringComparison.InvariantCulture))
                {
                    inSubPath[i] = inSubPath[i].Remove(0, ModlistData.RulesTagFile.Length).TrimStart();
                }
            }
        }

        private bool IsRestOfEnabledModsContainsSameFile(HashSet<string> alreadyChecked, string inSubPath)
        {
            foreach (var modFile in ModlistData.EnabledModNamesList)
            {
                var mFilePath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modFile) + Path.DirectorySeparatorChar + inSubPath;
                if (!alreadyChecked.Contains(modFile) && File.Exists(mFilePath))
                {
                    //when one of other enabled mods already have same file
                    return true;
                }
            }
            return false;
        }

        protected bool ParseRules(string[] rules)
        {
            bool allIsTrue = false;
            foreach (var rule in rules)
            {
                if (rule.StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture))
                {
                    allIsTrue = ParseReq(ModlistData.ModName, rule);
                }
                else if (rule.StartsWith(ModlistData.RulesTagInc, StringComparison.InvariantCulture))
                {
                    allIsTrue = ParseInc(ModlistData.ModName, rule);
                }
                if (!allIsTrue)
                {
                    break;
                }
            }
            return allIsTrue;
        }

        private bool ParseInc(string modname, string rule)
        {
            bool allIsTrue = false;
            var ruleData = rule.Remove(0, 4).Trim();
            if (ruleData.Contains(ModlistData.RulesTagOr) || ruleData.Contains(ModlistData.RulesTagAnd))
            {
                var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagOr, ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subRule in ruleDatas)
                {
                    allIsTrue = ParseIncSearchInEnabledMods(modname, subRule);
                    if (!allIsTrue)
                    {
                        return false;
                    }
                }
            }
            else
            {
                allIsTrue = ParseIncSearchInEnabledMods(modname, ruleData);
            }
            return allIsTrue;
        }

        private bool ParseIncSearchInEnabledMods(string modname, string ruleData)
        {
            if (ruleData.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
            {
                return ParseIncSearchFileInEnabledMods(modname, ruleData);
            }
            else
            {
                return ParseIncSearchModNameInEnabledMods(modname, ruleData);
            }
        }

        private bool ParseIncSearchModNameInEnabledMods(string modname, string ruleData)
        {
            if (ModlistData.EnabledModNamesList.Contains(ruleData))
            {
                //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                //{
                //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                //}
                if (!ModlistData.ModsMustBeDisabled.ContainsKey(modname))
                {
                    ModlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
                }
                return true;
            }
            return false;
        }

        private bool ParseIncSearchFileInEnabledMods(string modname, string ruleData)
        {
            ruleData = ruleData.Remove(0, 5).TrimStart();
            if (ManageModOrganizer.IsFileDirExistsInDataOrOverwrite(ruleData, out _))
            {
                //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                //{
                //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                //}
                if (!ModlistData.ModsMustBeDisabled.ContainsKey(modname))
                {
                    ModlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
                }
                return true;
            }
            foreach (var enabledModName in ModlistData.EnabledModNamesList)
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, enabledModName);
                var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                {
                    //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                    //{
                    //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                    //}
                    if (!ModlistData.ModsMustBeDisabled.ContainsKey(modname))
                    {
                        ModlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
                    }
                    return true;
                }
            }
            return false;
        }

        protected void AddCandidates()
        {
            AddCandidatesToMain(ModlistData.ModsMustBeEnabledCandidates, ModlistData.ModsMustBeEnabled);
            AddCandidatesToMain(ModlistData.ModsMustBeDisabledCandidates, ModlistData.ModsMustBeDisabled);
        }

        private static void AddCandidatesToMain(Dictionary<string, string> candidates, Dictionary<string, string> parent)
        {
            if (candidates.Count > 0)
            {
                foreach (var candidate in candidates)
                {
                    if (!parent.ContainsKey(candidate.Key))
                    {
                        parent.Add(candidate.Key, candidate.Value);
                    }
                }
            }
        }

        private bool ParseReq(string modname, string rule)
        {
            var ruleData = rule.StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture) ? rule.Remove(0, 4).Trim() : rule;
            var or = ruleData.Contains(ModlistData.RulesTagOr);
            var and = ruleData.Contains(ModlistData.RulesTagAnd);
            if (or && and)
            {
                return ParseReqorand(ruleData, modname);
            }
            else if (or)
            {
                return ParseReqor(ruleData, modname);
            }
            else if (and)
            {
                return ParseReqand(ruleData, modname);
            }
            else
            {
                return ParseReqSearchFileModNameInMods(modname, ruleData);
            }
        }

        private bool ParseReqorand(string ruleData, string modname)
        {
            //"modsA|or|modB|and|modC"
            //"modsA|and|modB|or|modC"
            //"modsA|or|modB|and|modC|or|modD"
            //"modsA|or|modB|and|modC|and|modD"
            //"modsA|and|modB|or|modC|or|modD"
            //"modsA|or|modB|or|modC|and|modD"
            //"modsA|and|modB|and|modC|or|modD"

            //минимум 3 части для OR и AND
            //минимум 2 нахождения OR и AND
            var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagOr, ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
            var orandMatches = Regex.Matches(ruleData, "(" + FixForRegex(ModlistData.RulesTagOr) + ")|(" + FixForRegex(ModlistData.RulesTagAnd) + ")");
            var maxParts = ruleDatas.Length;
            var matchesCount = orandMatches.Count;
            var partNumber = 0;
            string rule = string.Empty;
            for (int n = 0; n < matchesCount; n++)
            {
                //"modsA|or|modB|and|modC"
                //"modsA|and|modB|or|modC"
                if (orandMatches[n].Value == ModlistData.RulesTagOr)
                {
                    rule += ruleDatas[partNumber] + ModlistData.RulesTagOr;
                    partNumber++;
                }
                else if (orandMatches[n].Value == ModlistData.RulesTagAnd)
                {
                    rule += ruleDatas[partNumber];
                    if (!ParseReq(modname, rule))
                    {
                        return false;
                    }
                    rule = string.Empty;
                    partNumber++;
                }

                if (partNumber == maxParts - 1)
                {
                    if (!ParseReq(modname, rule += ruleDatas[partNumber]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static string FixForRegex(string str)
        {
            return str.Replace("|", "\\|");
        }

        private bool ParseReqand(string ruleData, string modname, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subRule in ruleDatas)
            {
                if (!ParseReqSearchFileModNameInMods(modname, subRule, 1))
                {
                    return false;
                }
            }
            //if (addCandidates)
            //{
            //    AddCandidates();
            //}
            return true;
        }

        private bool ParseReqor(string ruleData, string modname, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagOr }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subRule in ruleDatas)
            {
                if (ParseReqSearchFileModNameInMods(modname, subRule, 2))
                {
                    //if (addCandidates)
                    //{
                    //    AddCandidates();
                    //}
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Search required mod or file
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="ruleData"></param>
        /// <param name="modeAndor">0 - none, 1 - AND, 2 - OR</param>
        /// <returns></returns>
        private bool ParseReqSearchFileModNameInMods(string modname, string ruleData, int modeAndor = 0)
        {
            bool isAnyTrue;
            if (ruleData.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
            {
                isAnyTrue = ParseReqSearchFileInMods(modname, ruleData, modeAndor);
            }
            else
            {
                isAnyTrue = ParseReqSearchModNameInMods(modname, ruleData, modeAndor);
            }
            return isAnyTrue;
        }

        private bool ParseReqSearchModNameInMods(string modname, string ruleData, int modeAndor = 0)
        {
            if (!ModlistData.EnabledModNamesList.Contains(ruleData))
            {
                if (ModlistData.AllModNamesList.Contains(ruleData))
                {
                    if (modeAndor > 0)
                    {
                        if (!ModlistData.ModsMustBeEnabledCandidates.ContainsKey(ruleData))
                        {
                            ModlistData.ModsMustBeEnabledCandidates.Add(ruleData, modname + ">req:" + ruleData);
                        }
                    }
                    else
                    {
                        if (!ModlistData.ModsMustBeEnabled.ContainsKey(ruleData))
                        {
                            ModlistData.ModsMustBeEnabled.Add(ruleData, modname + ">req:" + ruleData);
                        }
                    }
                    return true;
                }
                else
                {
                    if (!ModlistData.ModsMustBeDisabled.ContainsKey(modname))
                    {
                        ModlistData.ModsMustBeDisabled.Add(modname, modname + ">req:" + ruleData);
                    }
                    //if (modeANDOR > 0)
                    //{
                    //    if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                    //    {
                    //        modlistData.ModsMustBeDisabledCandidates.Add(modname);
                    //    }
                    //}
                    //else
                    //{
                    //    if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                    //    {
                    //        modlistData.ModsMustBeDisabledCandidates.Add(modname);
                    //    }
                    //}
                }
            }
            return false;
        }

        private bool ParseReqSearchFileInMods(string modname, string ruleData, int modeAndor = 0)
        {
            return FindModWithThePath(ruleData, out _, modeAndor);

            //ruleData = ruleData.Remove(0, 5).TrimStart();
            //if (ManageMOMods.IsFileDirExistsInDataOROverwrite(ruleData, out _))
            //{
            //    return true;
            //}
            //foreach (var SubModName in modlistData.AllModsList)
            //{
            //    var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName);
            //    var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
            //    if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
            //    {
            //        if (!modlistData.EnabledModsList.Contains(SubModName))
            //        {
            //            if (modeANDOR > 0)
            //            {
            //                if (!modlistData.ModsMustBeEnabledCandidates.Contains(ruleData))
            //                {
            //                    modlistData.ModsMustBeEnabledCandidates.Add(ruleData);
            //                }
            //            }
            //            else
            //            {
            //                if (!modlistData.ModsMustBeEnabled.Contains(modname))
            //                {
            //                    modlistData.ModsMustBeEnabled.Add(modname);
            //                }
            //            }
            //        }
            //        return true;
            //    }
            //}
            //return false;
        }

        protected bool IsModListRule(string line)
        {
            return line.StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture)
                        ||
                   line.StartsWith(ModlistData.RulesTagInc, StringComparison.InvariantCulture);
        }
    }
}
