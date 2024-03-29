﻿using CheckForEmptyDir;
using System.IO;

namespace AIHelper.Manage.Rules.ModList
{
    internal class RuleModForOverlaysPlugin : ModListRulesBase
    {
        public RuleModForOverlaysPlugin(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool Condition()
        {
            return ModlistData.GamePrefix.Length > 0 && (
                !Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.ModName, "Overlays").IsNullOrEmptyDirectory("*.png", null, true
                )
                    )
                    && !File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.ModName, "BepInEx", "plugins", ModlistData.GamePrefix + "_OverlayMods.dll"));
        }

        internal override string Description()
        {
            return "Mod requires Overlays plugin";
        }

        internal override bool Fix()
        {
            return FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar + ModlistData.GamePrefix + "_OverlayMods.dll", out OutModName);
        }
    }
}
