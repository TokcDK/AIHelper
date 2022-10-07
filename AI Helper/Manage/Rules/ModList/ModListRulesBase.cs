using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AIHelper.Data.Modlist;
using NLog;

namespace AIHelper.Manage.Rules.ModList
{
    internal abstract class ModListRulesBase
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();

        protected ModListRulesData ModlistData;

        internal string OutModName;
        protected ModListRulesBase(ModListRulesData modlistData)
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
        /// <param name="foundMod">mod name of found mod</param>
        /// <param name="modeAndor">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string inSubPath, out ModData foundMod, int modeAndor = 0, bool dontAddCandidate = false)
        {
            return FindModWithThePath(new[] { inSubPath }, out foundMod, modeAndor, dontAddCandidate);
        }

        /// <summary>
        /// True if mod with the inSubPath will be found
        /// </summary>
        /// <param name="inSubPath">input path for search</param>
        /// <param name="foundMod">mod name of found mod</param>
        /// <param name="modeAndor">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string[] inSubPath, out ModData foundMod, int modeAndor = 0, bool dontAddCandidate = false)
        {
            RemoveRulesTagFile(ref inSubPath);

            if (File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name) + Path.DirectorySeparatorChar + inSubPath))
            {
                foundMod = ModlistData.Mod;
                return true;
            }

            if (IsExistInDataOrOverwrite(inSubPath, out foundMod))
            {
                return true;
            }

            //AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
            //EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

            foundMod = null;
            var alreadyChecked = new HashSet<ModData>();
            foreach (var subMod in ModlistData.AllModNamesList)
            {
                ///add modname to already checked
                if (!alreadyChecked.Contains(subMod))
                {
                    alreadyChecked.Add(subMod);
                }

                for (int i = 0; i < inSubPath.Length; i++)
                {
                    var filePath = Path.Combine(ManageSettings.CurrentGameModsDirPath, subMod.Name) + Path.DirectorySeparatorChar + inSubPath[i];
                    if (subMod != ModlistData.Mod && File.Exists(filePath))
                    {
                        foundMod = subMod;

                        //when mod enabled return true and not add
                        if (ModlistData.EnabledModNamesList.Contains(foundMod))
                        {
                            return true;
                        }

                        ///Check if path not exists in rest of Enabled mods
                        if (IsRestOfEnabledModsContainsSameFile(alreadyChecked, inSubPath[i])) return true;

                        //else if mod not enabled then add it for activation
                        if (!dontAddCandidate)
                        {
                            if (!foundMod.IsEnabled)
                            {
                                foundMod.IsEnabled = true;
                                foundMod.ReportMessages.Add($"Enabled, required by: {ModlistData.Mod.Name}");
                            }

                            //if (modeANDOR > 0)
                            //{
                            //    if (!ModlistData.Mod.NamesMustBeEnabledCandidates.ContainsKey(FoundModName))
                            //    {
                            //        ModlistData.Mod.NamesMustBeEnabledCandidates.Add(FoundModName, "req:" + SubModName);
                            //    }
                            //}
                            //else
                            //{
                            //    if (!ModlistData.Mod.NamesMustBeEnabledCandidates.ContainsKey(FoundModName))
                            //    {
                            //        ModlistData.Mod.NamesMustBeEnabledCandidates.Add(FoundModName, "req:" + SubModName);
                            //    }
                            //    //if (!ModlistData.Mod.NamesMustBeEnabled.Contains(FoundModName))
                            //    //{
                            //    //    ModlistData.Mod.NamesMustBeEnabled.Add(FoundModName);
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
            if (ModlistData.Mod.IsEnabled)
            {
                ModlistData.Mod.IsEnabled = false;
                ModlistData.Mod.ReportMessages.Add($"Disabled, requires: {string.Join(",", inSubPath)}");
            }

            return false;
        }

        private bool IsExistInDataOrOverwrite(string[] inSubPath, out ModData foundMod)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                var mod = ModlistData.EnabledModNamesList.FirstOrDefault(m => File.Exists(m.Path + "\\" + inSubPath[i]) || Directory.Exists(m.Path + "\\" + inSubPath[i]));
                if (mod == null) continue;

                foundMod = mod;
                return true;
            }
            foundMod = null;
            return false;
        }

        private void RemoveRulesTagFile(ref string[] inSubPath)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (!inSubPath[i].TrimStart().StartsWith(ModlistData.RulesTagFile, System.StringComparison.InvariantCulture)) continue;

                inSubPath[i] = inSubPath[i].Remove(0, ModlistData.RulesTagFile.Length).TrimStart();
            }
        }

        private bool IsRestOfEnabledModsContainsSameFile(HashSet<ModData> alreadyChecked, string inSubPath)
        {
            foreach (var mod in ModlistData.EnabledModNamesList)
            {
                var mFilePath = Path.Combine(ManageSettings.CurrentGameModsDirPath, mod.Name) + Path.DirectorySeparatorChar + inSubPath;
                
                if (!alreadyChecked.Contains(mod) && File.Exists(mFilePath)) return true;//when one of other enabled mods already have same file
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
                    allIsTrue = ParseReq(ModlistData.Mod, rule);
                }
                else if (rule.StartsWith(ModlistData.RulesTagInc, StringComparison.InvariantCulture))
                {
                    allIsTrue = ParseInc(ModlistData.Mod, rule);
                }

                if (!allIsTrue) break;
            }
            return allIsTrue;
        }

        private bool ParseInc(ModData mod, string rule)
        {
            bool allIsTrue = false;
            var ruleData = rule.Remove(0, 4).Trim();
            if (ruleData.Contains(ModlistData.RulesTagOr) || ruleData.Contains(ModlistData.RulesTagAnd))
            {
                var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagOr, ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subRule in ruleDatas)
                {
                    allIsTrue = ParseIncSearchInEnabledMods(mod, subRule);
                    if (!allIsTrue)
                    {
                        return false;
                    }
                }
            }
            else
            {
                allIsTrue = ParseIncSearchInEnabledMods(mod, ruleData);
            }
            return allIsTrue;
        }

        private bool ParseIncSearchInEnabledMods(ModData mod, string ruleData)
        {
            if (ruleData.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
            {
                return ParseIncSearchFileInEnabledMods(mod, ruleData);
            }
            else
            {
                return ParseIncSearchModNameInEnabledMods(mod, ruleData);
            }
        }

        private bool ParseIncSearchModNameInEnabledMods(ModData mod, string modName)
        {
            if (ModlistData.EnabledModNamesList.Any(m => m.Name == modName))
            {
                //if (!ModlistData.Mod.NamesMustBeDisabledCandidates.Contains(modname))
                //{
                //    ModlistData.Mod.NamesMustBeDisabledCandidates.Add(modname);
                //}
                if (mod.IsEnabled)
                {
                    mod.IsEnabled = false;
                    mod.ReportMessages.Add($"Disabled, requires mod {modName}");
                }
                return true;
            }
            return false;
        }

        private bool ParseIncSearchFileInEnabledMods(ModData mod, string modName)
        {
            modName = modName.Remove(0, 5).TrimStart();
            if (ModlistData.EnabledModNamesList.Any(m => m.Name == modName))
            {
                //if (!ModlistData.Mod.NamesMustBeDisabledCandidates.Contains(modname))
                //{
                //    ModlistData.Mod.NamesMustBeDisabledCandidates.Add(modname);
                //}
                if (mod.IsEnabled)
                {
                    mod.IsEnabled = false;
                    mod.ReportMessages.Add($"Disabled, incompatible with mod {modName}");
                }
                return true;
            }
            foreach (var enabledMod in ModlistData.EnabledModNamesList)
            {
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, enabledMod.Name);
                var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + modName);
                if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                {
                    //if (!ModlistData.Mod.NamesMustBeDisabledCandidates.Contains(modname))
                    //{
                    //    ModlistData.Mod.NamesMustBeDisabledCandidates.Add(modname);
                    //}
                    if (mod.IsEnabled)
                    {
                        mod.IsEnabled = false;
                        mod.ReportMessages.Add($"Disabled, incompatible with mod {modName}");
                    }
                    return true;
                }
            }
            return false;
        }

        protected void AddCandidates()
        {
            //AddCandidatesToMain(ModlistData.Mod.NamesMustBeEnabledCandidates, ModlistData.Mod.NamesMustBeEnabled);
            //AddCandidatesToMain(ModlistData.Mod.NamesMustBeDisabledCandidates, ModlistData.Mod.NamesMustBeDisabled);
        }

        //private static void AddCandidatesToMain(Dictionary<string, string> candidates, Dictionary<string, string> parent)
        //{
        //    if (candidates.Count > 0)
        //    {
        //        foreach (var candidate in candidates)
        //        {
        //            if (!parent.ContainsKey(candidate.Key))
        //            {
        //                parent.Add(candidate.Key, candidate.Value);
        //            }
        //        }
        //    }
        //}

        private bool ParseReq(ModData mod, string rule)
        {
            var ruleData = rule.StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture) ? rule.Remove(0, 4).Trim() : rule;
            var or = ruleData.Contains(ModlistData.RulesTagOr);
            var and = ruleData.Contains(ModlistData.RulesTagAnd);
            if (or && and)
            {
                return ParseReqorand(ruleData, mod);
            }
            else if (or)
            {
                return ParseReqor(ruleData, mod);
            }
            else if (and)
            {
                return ParseReqand(ruleData, mod);
            }
            else
            {
                return ParseReqSearchFileModNameInMods(mod, ruleData);
            }
        }

        private bool ParseReqorand(string ruleData, ModData mod)
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
                    if (!ParseReq(mod, rule))
                    {
                        return false;
                    }
                    rule = string.Empty;
                    partNumber++;
                }

                if (partNumber == maxParts - 1)
                {
                    if (!ParseReq(mod, rule += ruleDatas[partNumber]))
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

        private bool ParseReqand(string ruleData, ModData mod, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subRule in ruleDatas)
            {
                if (!ParseReqSearchFileModNameInMods(mod, subRule, 1)) return false;
            }
            //if (addCandidates)
            //{
            //    AddCandidates();
            //}
            return true;
        }

        private bool ParseReqor(string ruleData, ModData mod, bool addCandidates = false)
        {
            var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagOr }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subRule in ruleDatas)
            {
                if (ParseReqSearchFileModNameInMods(mod, subRule, 2))
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
        /// <param name="mod"></param>
        /// <param name="ruleData"></param>
        /// <param name="modeAndor">0 - none, 1 - AND, 2 - OR</param>
        /// <returns></returns>
        private bool ParseReqSearchFileModNameInMods(ModData mod, string ruleData, int modeAndor = 0)
        {
            bool isAnyTrue;
            if (ruleData.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
            {
                isAnyTrue = ParseReqSearchFileInMods(mod, ruleData, modeAndor);
            }
            else
            {
                isAnyTrue = ParseReqSearchModNameInMods(mod, ruleData, modeAndor);
            }
            return isAnyTrue;
        }

        private bool ParseReqSearchModNameInMods(ModData mod, string modName, int modeAndor = 0)
        {
            var targetMod = ModlistData.AllModNamesList.FirstOrDefault(m => m.Name == modName);

            if (targetMod != null)
            {
                if (!targetMod.IsEnabled)
                {
                    targetMod.IsEnabled = true;
                    targetMod.ReportMessages.Add($"Enabled, required by: {mod.Name}");
                }
                return true;
            }
            else
            {
                if (mod.IsEnabled)
                {
                    mod.IsEnabled = false;
                    targetMod.ReportMessages.Add($"Disabled, requires mod {modName}");
                }
            }
            return false;
        }

        private bool ParseReqSearchFileInMods(ModData mod, string ruleData, int modeAndor = 0)
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
            //                if (!ModlistData.Mod.NamesMustBeEnabledCandidates.Contains(ruleData))
            //                {
            //                    ModlistData.Mod.NamesMustBeEnabledCandidates.Add(ruleData);
            //                }
            //            }
            //            else
            //            {
            //                if (!ModlistData.Mod.NamesMustBeEnabled.Contains(modname))
            //                {
            //                    ModlistData.Mod.NamesMustBeEnabled.Add(modname);
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
