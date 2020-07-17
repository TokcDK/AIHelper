using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromRulesFile : ModListRules
    {
        public RuleFromRulesFile(ModListData modlistData) : base(modlistData)
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

            if (modlistData.rulesDict == null && !FillrulesDict(out modlistData.rulesDict))
            {
                return false;
            }
            if (modlistData.rulesDict.ContainsKey(modlistData.ModName))
            {
                var t = modlistData.rulesDict[modlistData.ModName][1];
            }

            return modlistData.rulesDict.ContainsKey(modlistData.ModName);
        }

        internal override string Description()
        {
            return "Rules from file";
        }

        internal override bool Fix()
        {
            return ParseRulesFromFile();
        }

        private bool ParseRulesFromFile()
        {
            return ParseRules(modlistData.rulesDict[modlistData.ModName]);
        }

        private bool FillrulesDict(out Dictionary<string, string[]> rulesDict)
        {
            var modlistRulesPath = ManageSettings.GetCurrentGameModListRulesPath();
            if (!File.Exists(modlistRulesPath))
            {
                rulesDict = null;
                return false;
            }

            rulesDict = new Dictionary<string, string[]>();
            string ruleName = string.Empty;
            List<string> ruleConditions = new List<string>();
            using (var sr = new StreamReader(modlistRulesPath))
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
                    else
                    {
                        if (IsModListRule(line))
                        {
                            ruleConditions.Add(line.Trim());
                        }
                        else
                        {
                            ruleName = string.Empty;
                            ruleConditions.Clear();
                            ruleReading = false;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                {
                    rulesDict.Add(ruleName, ruleConditions.ToArray());
                }
            }

            return rulesDict.Count > 0;
        }
    }
}
