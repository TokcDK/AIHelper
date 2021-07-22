using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Rules.ModList
{
    internal abstract class ModListRulesBase
    {
        protected ModListData modlistData;

        internal string outModName;
        protected ModListRulesBase(ModListData modlistData)
        {
            this.modlistData = modlistData;
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
        /// <param name="FoundModName">mod name of found mod</param>
        /// <param name="modeANDOR">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string inSubPath, out string FoundModName, int modeANDOR = 0, bool DontAddCandidate = false)
        {
            return FindModWithThePath(new[] { inSubPath }, out FoundModName, modeANDOR, DontAddCandidate);
        }

        /// <summary>
        /// True if mod with the inSubPath will be found
        /// </summary>
        /// <param name="inSubPath">input path for search</param>
        /// <param name="FoundModName">mod name of found mod</param>
        /// <param name="modeANDOR">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string[] inSubPath, out string FoundModName, int modeANDOR = 0, bool DontAddCandidate = false)
        {
            RemoveRulesTagFile(ref inSubPath);

            if (File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName) + Path.DirectorySeparatorChar + inSubPath))
            {
                FoundModName = modlistData.ModName;
                return true;
            }

            if (IsExistInDataOrOverwrite(inSubPath, out FoundModName))
            {
                return true;
            }

            //AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
            //EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            FoundModName = string.Empty;
            HashSet<string> AlreadyChecked = new HashSet<string>();
            foreach (var SubModName in modlistData.AllModsList)
            {
                ///add modname to already checked
                if (!AlreadyChecked.Contains(SubModName))
                {
                    AlreadyChecked.Add(SubModName);
                }

                for (int i = 0; i < inSubPath.Length; i++)
                {
                    var filePath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), SubModName) + Path.DirectorySeparatorChar + inSubPath[i];
                    if (SubModName != modlistData.ModName && File.Exists(filePath))
                    {
                        FoundModName = SubModName;

                        //when mod enabled return true and not add
                        if (modlistData.EnabledModsList.Contains(FoundModName))
                        {
                            return true;
                        }

                        ///Check if path not exists in rest of Enabled mods
                        if (IsRestOfEnabledModsContainsSameFile(AlreadyChecked, inSubPath[i]))
                        {
                            return true;
                        }

                        //else if mod not enabled then add it for activation
                        if (!DontAddCandidate)
                        {
                            if (!modlistData.ModsMustBeEnabledCandidates.ContainsKey(FoundModName))
                            {
                                modlistData.ModsMustBeEnabledCandidates.Add(FoundModName, "req:" + SubModName);
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
            if (!modlistData.ModsMustBeDisabled.ContainsKey(modlistData.ModName))
            {
                modlistData.ModsMustBeDisabled.Add(modlistData.ModName, "req:" + string.Join(",", inSubPath));
            }

            return false;
        }

        private static bool IsExistInDataOrOverwrite(string[] inSubPath, out string FoundModName)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (ManageMOMods.IsFileDirExistsInDataOROverwrite(inSubPath[i], out string Source))
                {
                    FoundModName = Source;
                    return true;
                }
            }
            FoundModName = string.Empty;
            return false;
        }

        private void RemoveRulesTagFile(ref string[] inSubPath)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (inSubPath[i].TrimStart().StartsWith(modlistData.RulesTagFile, System.StringComparison.InvariantCulture))
                {
                    inSubPath[i] = inSubPath[i].Remove(0, modlistData.RulesTagFile.Length).TrimStart();
                }
            }
        }

        private bool IsRestOfEnabledModsContainsSameFile(HashSet<string> AlreadyChecked, string inSubPath)
        {
            foreach (var ModFile in modlistData.EnabledModsList)
            {
                var mFilePath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModFile) + Path.DirectorySeparatorChar + inSubPath;
                if (!AlreadyChecked.Contains(ModFile) && File.Exists(mFilePath))
                {
                    //when one of other enabled mods already have same file
                    return true;
                }
            }
            return false;
        }

        protected bool ParseRules(string[] rules)
        {
            bool AllIsTrue = false;
            foreach (var rule in rules)
            {
                if (rule.StartsWith(modlistData.RulesTagREQ, StringComparison.InvariantCulture))
                {
                    AllIsTrue = ParseREQ(modlistData.ModName, rule);
                }
                else if (rule.StartsWith(modlistData.RulesTagINC, StringComparison.InvariantCulture))
                {
                    AllIsTrue = ParseINC(modlistData.ModName, rule);
                }
                if (!AllIsTrue)
                {
                    break;
                }
            }
            return AllIsTrue;
        }

        private bool ParseINC(string modname, string rule)
        {
            bool AllIsTrue = false;
            var ruleData = rule.Remove(0, 4).Trim();
            if (ruleData.Contains(modlistData.RulesTagOR) || ruleData.Contains(modlistData.RulesTagAND))
            {
                var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR, modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var SubRule in ruleDatas)
                {
                    AllIsTrue = ParseINCSearchInEnabledMods(modname, SubRule);
                    if (!AllIsTrue)
                    {
                        return false;
                    }
                }
            }
            else
            {
                AllIsTrue = ParseINCSearchInEnabledMods(modname, ruleData);
            }
            return AllIsTrue;
        }

        private bool ParseINCSearchInEnabledMods(string modname, string ruleData)
        {
            if (ruleData.StartsWith(modlistData.RulesTagFile, StringComparison.InvariantCulture))
            {
                return ParseINCSearchFileInEnabledMods(modname, ruleData);
            }
            else
            {
                return ParseINCSearchModNameInEnabledMods(modname, ruleData);
            }
        }

        private bool ParseINCSearchModNameInEnabledMods(string modname, string ruleData)
        {
            if (modlistData.EnabledModsList.Contains(ruleData))
            {
                //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                //{
                //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                //}
                if (!modlistData.ModsMustBeDisabled.ContainsKey(modname))
                {
                    modlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
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
                //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                //{
                //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                //}
                if (!modlistData.ModsMustBeDisabled.ContainsKey(modname))
                {
                    modlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
                }
                return true;
            }
            foreach (var EnabledModName in modlistData.EnabledModsList)
            {
                var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), EnabledModName);
                var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleData);
                if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                {
                    //if (!modlistData.ModsMustBeDisabledCandidates.Contains(modname))
                    //{
                    //    modlistData.ModsMustBeDisabledCandidates.Add(modname);
                    //}
                    if (!modlistData.ModsMustBeDisabled.ContainsKey(modname))
                    {
                        modlistData.ModsMustBeDisabled.Add(modname, "inc:" + ruleData);
                    }
                    return true;
                }
            }
            return false;
        }

        protected void AddCandidates()
        {
            AddCandidatesToMain(modlistData.ModsMustBeEnabledCandidates, modlistData.ModsMustBeEnabled);
            AddCandidatesToMain(modlistData.ModsMustBeDisabledCandidates, modlistData.ModsMustBeDisabled);
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

        private bool ParseREQ(string modname, string rule)
        {
            var ruleData = rule.StartsWith(modlistData.RulesTagREQ, StringComparison.InvariantCulture) ? rule.Remove(0, 4).Trim() : rule;
            var or = ruleData.Contains(modlistData.RulesTagOR);
            var and = ruleData.Contains(modlistData.RulesTagAND);
            if (or && and)
            {
                return ParseREQORAND(ruleData, modname);
            }
            else if (or)
            {
                return ParseREQOR(ruleData, modname);
            }
            else if (and)
            {
                return ParseREQAND(ruleData, modname);
            }
            else
            {
                return ParseREQSearchFileModNameInMods(modname, ruleData);
            }
        }

        private bool ParseREQORAND(string ruleData, string modname)
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
            var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR, modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
            var ORANDMatches = Regex.Matches(ruleData, "(" + FixForRegex(modlistData.RulesTagOR) + ")|(" + FixForRegex(modlistData.RulesTagAND) + ")");
            var MaxParts = ruleDatas.Length;
            var MatchesCount = ORANDMatches.Count;
            var PartNumber = 0;
            string rule = string.Empty;
            for (int n = 0; n < MatchesCount; n++)
            {
                //"modsA|or|modB|and|modC"
                //"modsA|and|modB|or|modC"
                if (ORANDMatches[n].Value == modlistData.RulesTagOR)
                {
                    rule += ruleDatas[PartNumber] + modlistData.RulesTagOR;
                    PartNumber++;
                }
                else if (ORANDMatches[n].Value == modlistData.RulesTagAND)
                {
                    rule += ruleDatas[PartNumber];
                    if (!ParseREQ(modname, rule))
                    {
                        return false;
                    }
                    rule = string.Empty;
                    PartNumber++;
                }

                if (PartNumber == MaxParts - 1)
                {
                    if (!ParseREQ(modname, rule += ruleDatas[PartNumber]))
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

        private bool ParseREQAND(string ruleData, string modname, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagAND }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var SubRule in ruleDatas)
            {
                if (!ParseREQSearchFileModNameInMods(modname, SubRule, 1))
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

        private bool ParseREQOR(string ruleData, string modname, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { modlistData.RulesTagOR }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var SubRule in ruleDatas)
            {
                if (ParseREQSearchFileModNameInMods(modname, SubRule, 2))
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
                        if (!modlistData.ModsMustBeEnabledCandidates.ContainsKey(ruleData))
                        {
                            modlistData.ModsMustBeEnabledCandidates.Add(ruleData, modname + ">req:" + ruleData);
                        }
                    }
                    else
                    {
                        if (!modlistData.ModsMustBeEnabled.ContainsKey(ruleData))
                        {
                            modlistData.ModsMustBeEnabled.Add(ruleData, modname + ">req:" + ruleData);
                        }
                    }
                    return true;
                }
                else
                {
                    if (!modlistData.ModsMustBeDisabled.ContainsKey(modname))
                    {
                        modlistData.ModsMustBeDisabled.Add(modname, modname + ">req:" + ruleData);
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

        private bool ParseREQSearchFileInMods(string modname, string ruleData, int modeANDOR = 0)
        {
            return FindModWithThePath(ruleData, out _, modeANDOR);

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
            return line.StartsWith(modlistData.RulesTagREQ, StringComparison.InvariantCulture)
                        ||
                   line.StartsWith(modlistData.RulesTagINC, StringComparison.InvariantCulture);
        }
    }
}
