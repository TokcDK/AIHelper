using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromRulesFileOverall : ModListRules
    {
        public RuleFromRulesFileOverall(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool IsHardRule { get => false; }

        internal override bool Condition()
        {
            var RulesFilePath = ManageSettings.GetCurrentGameModListRulesPath();

            if (!File.Exists(RulesFilePath))
            {
                return false;
            }

            return FillrulesDictOverall();
        }

        internal override string Description()
        {
            return "Rules from file. Overal file exist check.";
        }

        internal override bool Fix()
        {
            var IsAllTrue = false;
            foreach (var file in modlistData.rulesDictOverall)
            {
                if(File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, file.Key.Remove(0, modlistData.RulesTagFile.Length)))
                    ||
                    Directory.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, file.Key.Remove(0, modlistData.RulesTagFile.Length)))
                    )
                {
                    IsAllTrue = ParseRules(file.Value);
                }
                if (!IsAllTrue)
                {
                    return false;
                }
            }
            return IsAllTrue;
        }

        private bool FillrulesDictOverall()
        {
            if (modlistData.rulesDictOverall != null)
            {
                return modlistData.rulesDictOverall.Count > 0;
            }

            if (modlistData.rulesDictOverall == null)
            {
                modlistData.rulesDictOverall = new Dictionary<string, string[]>();
            }

            var modlistRulesPath = ManageSettings.GetCurrentGameModListRulesPath();
            if (!File.Exists(modlistRulesPath))
            {
                modlistData.rulesDictOverall = null;
                return false;
            }

            string ruleName = string.Empty;
            List<string> ruleConditions = new List<string>();
            using (var sr = new StreamReader(modlistRulesPath))
            {
                bool ruleReading = false;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (ruleReading && IsModListRule(line))
                    {
                        ruleConditions.Add(line.Trim());
                    }
                    else if(ruleReading && !string.IsNullOrWhiteSpace(ruleName))
                    {
                        if (ruleConditions.Count > 0)
                        {
                            modlistData.rulesDictOverall.Add(ruleName, ruleConditions.ToArray());
                            ruleConditions.Clear();
                        }
                        ruleName = string.Empty;
                        ruleReading = false;
                    }
                    if (!ruleReading)
                    {
                        if (line.StartsWith(modlistData.RulesTagFile, StringComparison.InvariantCulture))
                        {
                            ruleName = line.Trim();
                            ruleReading = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                {
                    modlistData.rulesDictOverall.Add(ruleName, ruleConditions.ToArray());
                }
            }

            return modlistData.rulesDictOverall.Count > 0;
        }
    }
}
