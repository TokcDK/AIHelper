//using System.IO;
//using System.Linq;

//namespace AIHelper.Manage.Rules.ModList
//{
//    internal class RuleModForBepInexIPA : ModListRulesBase
//    {
//        public RuleModForBepInexIPA(ModListData modlistData) : base(modlistData)
//        {
//        }

//        internal override bool Condition()
//        {
//            return !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(
//                Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.Mod.NameName, "BepInEx", "IPA"), "*.dll", null, true
//                ) &&
//                 !File.Exists(Path.Combine(ManageSettings.GetCurrentGameModsPath(), ModlistData.Mod.NameName, "BepInEx", "patchers", "BepInEx.IPAVirtualizer.dll"));
//        }

//        internal override string Description()
//        {
//            return "Mod requires BepInEx IPA loader";
//        }

//        internal override bool Fix()
//        {
//            //List<string> modNamesToActivate = new List<string>();
//            if (FindModWithThePath("BepInEx" + Path.DirectorySeparatorChar + "patchers" + Path.DirectorySeparatorChar + "BepInEx.IPAVirtualizer.dll", out outModName))
//            {
//                if (!string.IsNullOrWhiteSpace(outModName) && outModName != "data" && outModName != "overwrite")
//                {
//                    //ManageMO.ActivateInsertModIfPossible(outModName);
//                    if (!modlistData.EnabledModsList.Contains(outModName))
//                    {
//                        Result = "was enabled required mod" + " \"" + outModName + "\"";
//                    }

//                    return true;
//                }
//            }

//            return false;
//        }
//    }
//}
