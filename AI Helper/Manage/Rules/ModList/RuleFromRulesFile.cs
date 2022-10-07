using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromRulesFile : ModListRulesBase
    {
        public RuleFromRulesFile(ModListRulesData modlistData) : base(modlistData)
        {
        }

        internal override bool IsHardRule { get => false; }

        internal override bool Condition()
        {
            var rulesFilePath = ManageSettings.CurrentGameModListRulesPath;

            if (!File.Exists(rulesFilePath)) return false;

            if (!FillrulesDict()) return false;
            //if (modlistData.rulesDict.ContainsKey(ModlistData.Mod.NameName))
            //{
            //    var t = modlistData.rulesDict[ModlistData.Mod.NameName][1];
            //}

            return ModlistData.RulesDict.ContainsKey(ModlistData.Mod.Name);
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
            return ParseRules(ModlistData.RulesDict[ModlistData.Mod.Name]);
        }

        private bool FillrulesDict()
        {
            if (ModlistData.RulesDict != null) return ModlistData.RulesDict.Count > 0;

            ModlistData.RulesDict = new Dictionary<string, string[]>();

            var modlistRulesPath = ManageSettings.CurrentGameModListRulesPath;
            if (!File.Exists(modlistRulesPath))
            {
                ModlistData.RulesDict = null;
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
                    if (string.IsNullOrWhiteSpace(line) || ModListRulesData.IsComment(line))
                    {
                        continue;
                    }

                    line = line.Trim();
                    if (ruleReading && IsModListRule(line))
                    {
                        ruleConditions.Add(ModListRulesData.GetDataWithNoComments(line));
                    }
                    else if (ruleReading && !string.IsNullOrWhiteSpace(ruleName))
                    {
                        if (ruleConditions.Count > 0)
                        {
                            ModlistData.RulesDict.Add(ruleName, ruleConditions.ToArray());
                            ruleConditions.Clear();
                        }
                        ruleName = string.Empty;
                        ruleReading = false;
                    }
                    if (!ruleReading)
                    {
                        if (!line.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
                        {
                            ruleName = ModListRulesData.GetDataWithNoComments(line);
                            ruleReading = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                {
                    ModlistData.RulesDict.Add(ruleName, ruleConditions.ToArray());
                }
            }

            return ModlistData.RulesDict.Count > 0;
        }
    }
}
