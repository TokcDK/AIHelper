﻿using System;
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

            if (!FillrulesDict())
            {
                return false;
            }
            //if (modlistData.rulesDict.ContainsKey(modlistData.ModName))
            //{
            //    var t = modlistData.rulesDict[modlistData.ModName][1];
            //}

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

        private bool FillrulesDict()
        {
            if (modlistData.rulesDict != null)
            {
                return modlistData.rulesDict.Count > 0;
            }

            if (modlistData.rulesDict == null)
            {
                modlistData.rulesDict = new Dictionary<string, string[]>();
            }

            var modlistRulesPath = ManageSettings.GetCurrentGameModListRulesPath();
            if (!File.Exists(modlistRulesPath))
            {
                modlistData.rulesDict = null;
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
                    if (string.IsNullOrWhiteSpace(line)
                        || line.TrimStart().StartsWith(";", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (ruleReading && IsModListRule(line))
                    {
                        ruleConditions.Add(line.Trim());
                    }
                    else if (ruleReading && !string.IsNullOrWhiteSpace(ruleName))
                    {
                        if (ruleConditions.Count > 0)
                        {
                            modlistData.rulesDict.Add(ruleName, ruleConditions.ToArray());
                            ruleConditions.Clear();
                        }
                        ruleName = string.Empty;
                        ruleReading = false;
                    }
                    if (!ruleReading)
                    {
                        if (!line.StartsWith(modlistData.RulesTagFile, StringComparison.InvariantCulture))
                        {
                            ruleName = line.Trim();
                            ruleReading = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                {
                    modlistData.rulesDict.Add(ruleName, ruleConditions.ToArray());
                }
            }

            return modlistData.rulesDict.Count > 0;
        }
    }
}