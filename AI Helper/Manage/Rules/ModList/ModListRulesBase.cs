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
        protected bool FindModWithThePath(string[] inSubPath, out ModData foundMod)
        {
            foundMod = null;

            foreach (var subPath in inSubPath)
            {
                if (!FindModWithThePath(subPath, out foundMod)) continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// True if mod with the inSubPath will be found
        /// </summary>
        /// <param name="inSubPath">input path for search</param>
        /// <param name="foundMod">mod name of found mod</param>
        /// <param name="modeAndor">0-none, 1-and, 2-or</param>
        /// <returns></returns>
        protected bool FindModWithThePath(string inSubPath, out ModData foundMod)
        {
            foundMod = null;

            if (File.Exists(ManageSettings.CurrentGameDataDirPath + "\\" + inSubPath)) return true;

            foreach (var mod in ModlistData.EnabledModsListAndOverwrite)
            {
                if (!File.Exists(mod.Path + Path.DirectorySeparatorChar + inSubPath)) continue;

                foundMod = mod;
                return true;
            }

            return false;

            //return FindModWithThePath(new[] { inSubPath }, out foundMod, modeAndor, dontAddCandidate);
        }

        private bool IsExistInDataOrOverwrite(string[] inSubPath)
        {
            for (int i = 0; i < inSubPath.Length; i++)
            {
                if (File.Exists(ManageSettings.CurrentGameDataDirPath + "\\" + inSubPath)) { }
                else if (File.Exists(ManageSettings.CurrentGameOverwriteFolderPath + "\\" + inSubPath)) { }
                else continue;

                return true;
            }

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
            foreach (var mod in ModlistData.EnabledModsListAndOverwrite)
            {
                var mFilePath = Path.Combine(ManageSettings.CurrentGameModsDirPath, mod.Name) + Path.DirectorySeparatorChar + inSubPath;

                if (!alreadyChecked.Contains(mod) && File.Exists(mFilePath)) return true;//when one of other enabled mods already have same file
            }
            return false;
        }

        protected bool ParseRules(string[] rules)
        {
            // add rules for incompatible and required
            var RulesListINC = new List<string>();
            var RulesListREQ = new List<string>();
            foreach (var rule in rules)
            {
                if (rule.TrimStart().StartsWith(ModlistData.RulesTagInc, StringComparison.InvariantCulture))
                {
                    RulesListINC.Add(rule);
                }
                else if (rule.TrimStart().StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture))
                {
                    RulesListREQ.Add(rule);
                }
            }

            // all incompatible mods must be disamled or missing
            foreach (var rule in RulesListINC) if (!ParseInc(ModlistData.Mod, rule)) return false;

            // all required mods must be enabled
            foreach (var rule in RulesListREQ) if (!ParseReq(ModlistData.Mod, rule)) return false;

            // when all incompatible is missing or disabled and all required is exist and enabled
            return true;
        }

        private bool ParseInc(ModData mod, string rule)
        {
            // return false when any incompatible mod found

            var ruleData = rule.StartsWith(ModlistData.RulesTagInc, StringComparison.InvariantCulture) ? rule.Remove(0, ModlistData.RulesTagReq.Length).TrimStart() : rule; // remove prefix if was not removed

            var andMembers = ruleData.Split(new[] { ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries); // split by AND tag
            foreach (var andMember in andMembers) if (IsFound(mod, andMember)) return false; // parse all AND tag parts

            return true;
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
            if (ModlistData.EnabledModsListAndOverwrite.Any(m => m.Name == modName))
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
            if (ModlistData.EnabledModsListAndOverwrite.Any(m => m.Name == modName))
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
            foreach (var enabledMod in ModlistData.EnabledModsListAndOverwrite)
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
            // return false when any required mod is missing

            var ruleData = rule.StartsWith(ModlistData.RulesTagReq, StringComparison.InvariantCulture) ? rule.Remove(0, ModlistData.RulesTagReq.Length).TrimStart() : rule; // remove prefix if was not removed
            //var or = ruleData.Contains(ModlistData.RulesTagOr);
            //var and = ruleData.Contains(ModlistData.RulesTagAnd);

            var andMembers = ruleData.Split(new[] { ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries); // split by AND tag
            foreach (var andMember in andMembers) if (!ParseReqAnd(andMember, mod)) return false; // parse all AND tag parts

            //if (or && and)
            //{
            //    return ParseReqorand(ruleData, mod);
            //}
            //else if (or)
            //{
            //    return ParseReqor(ruleData, mod);
            //}
            //else if (and)
            //{
            //    return ParseReqand(ruleData, mod);
            //}
            //else
            //{
            //    return ParseReqSearchFileModNameInMods(mod, ruleData);
            //}

            return true;
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

        private bool ParseReqAnd(string ruleData, ModData mod, bool addCandidates = false)
        {
            // parse OR tag parts
            var membersOfTypeOr = ruleData.Split(new[] { ModlistData.RulesTagOr }, StringSplitOptions.RemoveEmptyEntries);
            if (membersOfTypeOr.Length > 1)
            {
                foreach (var memberOfTypeOr in membersOfTypeOr) if (!ParseReqSearchFileModNameInMods(mod, memberOfTypeOr, 1)) return false;
            }
            else if (!IsFound(mod, ruleData)) return false; // when OR missing, parse single AND

            //foreach (var andMember in membersOfTypeOr) if (!ParseReqAnd(andMember, mod)) return false;

            //var ruleDatas = ruleData.Split(new[] { ModlistData.RulesTagAnd }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var subRule in ruleDatas)
            //{
            //    if (!ParseReqSearchFileModNameInMods(mod, subRule, 1)) return false;
            //}
            //if (addCandidates)
            //{
            //    AddCandidates();
            //}
            return true;
        }

        /// <summary>
        /// check if subpath or mod is exists
        /// </summary>
        /// <param name="targetMod"></param>
        /// <param name="ruleData"></param>
        /// <returns></returns>
        private bool IsFound(ModData targetMod, string ruleData)
        {
            bool isFileSubPath = false;
            if (isFileSubPath = ruleData.TrimStart().StartsWith(ModlistData.RulesTagFile))
            {
                ruleData = ruleData.TrimStart().Substring(ModlistData.RulesTagFile.Length);
            }
            else if (ruleData.Contains("\\") || ruleData.Contains("/")) isFileSubPath = true;

            foreach (var mod in ModlistData.AllModsList)
            {
                if (isFileSubPath && File.Exists(mod.Path + "\\" + ruleData)) return true;
                if (!isFileSubPath)
                {
                    var m = ModlistData.AllModsList.FirstOrDefault(m1 => m1.Name == ruleData);
                    if (m != null)
                    {
                        targetMod.Relations.Requires.Add(m);
                        return true;
                    }
                }
            }

            return false;
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
            var targetMod = ModlistData.AllModsList.FirstOrDefault(m => m.Name == modName);

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

    public enum MemberType
    {
        AND,
        OR
    }
}
