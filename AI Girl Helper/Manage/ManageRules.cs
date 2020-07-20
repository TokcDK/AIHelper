using AIHelper.Manage.Rules.ModList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    internal class ManageRules
    {
        internal class ModList
        {
            ModListData modlistData;
            public ModList()
            {
                modlistData = new ModListData();
            }

            internal void ModlistFixes()
            {
                var ModlistBackupFilePath = ManageMO.MakeMOProfileModlistFileBuckup("_PreFixes");

                modlistData.AllModsList = ManageMO.GetModNamesListFromActiveMOProfile(false);
                modlistData.EnabledModsList = ManageMO.GetModNamesListFromActiveMOProfile();

                foreach (var ModName in modlistData.EnabledModsList)
                {
                    modlistData.ModName = ModName;
                    ApplyRules();
                }

                var ListChanged = false;
                modlistData.Report.Add(Environment.NewLine + T._("Actions") + ":");
                if (modlistData.ModsMustBeEnabled.Count > 0)
                {
                    foreach (var mod in modlistData.ModsMustBeEnabled)
                    {
                        modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was activated"));
                        ManageMO.ActivateMod(mod);
                        ListChanged = true;
                    }
                }

                if (modlistData.ModsMustBeDisabled.Count > 0)
                {
                    foreach (var mod in modlistData.ModsMustBeDisabled)
                    {
                        modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was deactivated"));
                        ManageMO.DeactivateMod(mod);
                        ListChanged = true;
                    }
                }

                if (modlistData.Report.Count > 0)
                {
                    MessageBox.Show(T._("Results") + ":" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, modlistData.Report));
                }
                if (!ListChanged && !string.IsNullOrWhiteSpace(ModlistBackupFilePath))
                {
                    try
                    {
                        File.Delete(ModlistBackupFilePath);
                    }
                    catch
                    {
                    }
                }
            }

            private void ApplyRules()
            {
                foreach (var rule in modlistData.RulesList)
                {
                    if (rule.Condition())
                    {
                        var oldMustEnabledCount = modlistData.ModsMustBeEnabled.Count;
                        var oldMustEnabledCandidatesCount = modlistData.ModsMustBeEnabledCandidates.Count;
                        var oldMustDisaledCount = modlistData.ModsMustBeDisabled.Count;
                        var oldMustDisaledCandidatesCount = modlistData.ModsMustBeDisabledCandidates.Count;

                        if (rule.Fix())
                        {
                            if (!modlistData.ModsMustBeDisabledCandidates.Contains(modlistData.ModName) && !modlistData.ModsMustBeDisabled.Contains(modlistData.ModName))
                            {
                                AddCandidates();
                            }
                            else
                            {
                                modlistData.ModsMustBeEnabledCandidates.Clear();
                                AddCandidates();
                            }
                            if (oldMustEnabledCount != modlistData.ModsMustBeEnabled.Count
                                ||
                                oldMustEnabledCandidatesCount != modlistData.ModsMustBeEnabledCandidates.Count
                                ||
                                oldMustDisaledCount != modlistData.ModsMustBeDisabled.Count
                                ||
                                oldMustDisaledCandidatesCount != modlistData.ModsMustBeDisabledCandidates.Count
                                )
                            {
                                var report = T._("For mod") + " \"" + modlistData.ModName + "\" " + T._("was applied rule") + " \"" + rule.Description() + "\"" + (rule.Result.Length > 0 ? (" " + T._("with result") + ":" + " \" " + rule.Result) : rule.Result);
                                if (!modlistData.Report.Contains(report))
                                {
                                    modlistData.Report.Add(report);
                                }
                            }
                        }
                        else
                        {
                            ClearCandidates();
                        }
                    }
                }
            }

            private void ClearCandidates()
            {
                modlistData.ModsMustBeEnabledCandidates.Clear();
                modlistData.ModsMustBeDisabledCandidates.Clear();
            }

            private void AddCandidates()
            {
                AddCandidatesToMain(modlistData.ModsMustBeEnabledCandidates, modlistData.ModsMustBeEnabled);
                AddCandidatesToMain(modlistData.ModsMustBeDisabledCandidates, modlistData.ModsMustBeDisabled);
                ClearCandidates();
            }

            private static void AddCandidatesToMain(List<string> candidates, List<string> parent)
            {
                if (candidates.Count > 0)
                {
                    foreach (var candidate in candidates)
                    {
                        if (!parent.Contains(candidate))
                        {
                            parent.Add(candidate);
                        }
                    }
                }
            }
        }
    }
}
