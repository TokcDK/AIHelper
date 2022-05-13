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
            var rulesFilePath = ManageSettings.CurrentGameModListRulesPath;

            if (!File.Exists(rulesFilePath))
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
            var isAllTrue = false;
            foreach (var file in ModlistData.RulesDictOverall)
            {
                var targetPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.ModName, file.Key.Remove(0, ModlistData.RulesTagFile.Length));
                if (File.Exists(targetPath)
                    ||
                    Directory.Exists(targetPath)
                    )
                {
                    isAllTrue = ParseRules(file.Value);
                }
                if (!isAllTrue)
                {
                    return false;
                }
            }
            return isAllTrue;
        }

        private bool FillrulesDictOverall()
        {
            if (ModlistData.RulesDictOverall != null)
            {
                return ModlistData.RulesDictOverall.Count > 0;
            }

            ModlistData.RulesDictOverall = new Dictionary<string, string[]>();

            var modlistRulesPath = ManageSettings.CurrentGameModListRulesPath;
            if (!File.Exists(modlistRulesPath))
            {
                ModlistData.RulesDictOverall = null;
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
                            ModlistData.RulesDictOverall.Add(ruleName, ruleConditions.ToArray());
                            ruleConditions.Clear();
                        }
                        ruleName = string.Empty;
                        ruleReading = false;
                    }
                    if (!ruleReading)
                    {
                        if (line.StartsWith(ModlistData.RulesTagFile, StringComparison.InvariantCulture))
                        {
                            ruleName = ModListData.GetDataWithNoComments(line);
                            ruleReading = true;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ruleName) && ruleConditions.Count > 0)
                {
                    ModlistData.RulesDictOverall.Add(ruleName, ruleConditions.ToArray());
                }
            }

            return ModlistData.RulesDictOverall.Count > 0;
        }
    }
}
