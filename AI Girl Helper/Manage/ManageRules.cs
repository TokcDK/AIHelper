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
        }

        internal class ModList
        {
            internal static void ModlistFixes()
            {
                var modlistData = new ModListData();
                modlistData.AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
                modlistData.EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

                //Dictionary<string, string[]> rulesDict;
                var rulesExists = FillrulesDict(out Dictionary<string, string[]> rulesDict);
                var OverwritePath = ManageSettings.GetCurrentGameMOOverwritePath();

                //parse rules from meta.ini
                foreach (var ModName in modlistData.EnabledModsList)
                {
                    var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModName);

                    var metaPath = Path.Combine(ModPath, "meta.ini");
                    if (File.Exists(metaPath))
                    {
                        var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");
                        var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                        var req = false;
                        var inc = false;
                        if ((req = metaNotes.Contains("req:")) || (inc = metaNotes.Contains("inc:")))
                        {
                            var subrules = new List<string>();
                            foreach (var line in metaNotes.SplitToLines())
                            {
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    continue;
                                }

                                var trimmedLine = line.Trim();
                                if ((req && trimmedLine.StartsWith("req:", StringComparison.InvariantCulture))
                                    || (inc && trimmedLine.StartsWith("inc:", StringComparison.InvariantCulture))
                                    )
                                {
                                    subrules.Add(trimmedLine);
                                }
                            }
                            if (subrules.Count > 0)
                            {
                                ParseRules(modlistData, ModName, subrules.ToArray());
                                //rulesDict.Add(Mod, subrules.ToArray());
                            }
                        }
                    }
                }
            }

            private static void ParseRules(ModListData modlistData, string modname, string[] rules)
            {
                foreach (var rule in rules)
                {
                    if (rule.StartsWith("req:", StringComparison.InvariantCulture))
                    {
                        ParseREQ(modlistData, modname, rule);
                    }
                    else if (rule.StartsWith("inc:", StringComparison.InvariantCulture))
                    {
                        ParseINC(modlistData, modname, rule);
                    }
                }
            }

            private static void ParseINC(ModListData modlistData, string modname, string rule)
            {
                var ruleParts = rule.Remove(0, 4).Trim();
                var or = ruleParts.Contains("|or|");
                var and = ruleParts.Contains("|and|");
                if (or && and)
                {

                }
                else if (or)
                {

                }
                else if (and)
                {

                }
                else
                {
                    if (ruleParts.StartsWith("file:", StringComparison.InvariantCulture))
                    {
                        ruleParts= ruleParts.Remove(0, 5).TrimStart();

                        foreach(var EnabledModName in modlistData.EnabledModsList)
                        {
                            var modPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), EnabledModName);
                            var targetfilePath = Path.GetFullPath(modPath + Path.DirectorySeparatorChar + ruleParts);
                            if (File.Exists(targetfilePath) || Directory.Exists(targetfilePath))
                            {
                                if(!modlistData.ModsMustBeDisabled.Contains(modname))
                                {
                                    modlistData.ModsMustBeDisabled.Add(modname);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (modlistData.EnabledModsList.Contains(ruleParts))
                        {
                            if (!modlistData.ModsMustBeDisabled.Contains(modname))
                            {
                                modlistData.ModsMustBeDisabled.Add(modname);
                            }
                        }
                    }
                }
            }

            private static void ParseREQ(ModListData modlistData, string modname, string rule)
            {
                throw new NotImplementedException();
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
