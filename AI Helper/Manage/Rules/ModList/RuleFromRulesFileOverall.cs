using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromRulesFileOverall : ModListRulesBase
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
                var targetPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName, file.Key.Remove(0, modlistData.RulesTagFile.Length));
                if (File.Exists(targetPath)
                    ||
                    Directory.Exists(targetPath)
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

            modlistData.rulesDictOverall = new Dictionary<string, string[]>();

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
                    if (string.IsNullOrWhiteSpace(line) || ModListData.IsComment(line))
                    {
                        continue;
                    }

                    line = line.Trim();
                    if (ruleReading && IsModListRule(line))
                    {
                        ruleConditions.Add(ModListData.GetDataWithNoComments(line));
                    }
                    else if (ruleReading && !string.IsNullOrWhiteSpace(ruleName))
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
                            ruleName = ModListData.GetDataWithNoComments(line);
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
