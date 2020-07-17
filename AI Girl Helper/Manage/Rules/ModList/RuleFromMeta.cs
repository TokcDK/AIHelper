﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromMeta : ModListRules
    {
        public RuleFromMeta(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool IsHardRule { get => false; }

        internal override bool Condition()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (!File.Exists(metaPath))
                return false;

            var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");

            return metaNotes.Contains("mlinfo::");
        }

        internal override string Description()
        {
            return "Rules from meta.ini of the mod";
        }

        internal override bool Fix()
        {
            return ParseRulesFromMeta();
        }

        private bool ParseRulesFromMeta()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (File.Exists(metaPath))
            {
                var metaNotes = ManageINI.GetINIValueIfExist(metaPath, "notes", "General");
                //var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                var mlinfo = ManageHTML.GetMLInfoTextFromHTML(metaNotes).Replace("\\\\","\\");

                if (!string.IsNullOrWhiteSpace(mlinfo))
                {
                    return ParseRules(mlinfo.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            return false;
        }
    }
}
