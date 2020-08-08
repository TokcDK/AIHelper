using AIHelper.Manage.Rules.ModList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

                var ListChanged = modlistData.Report.Count > 0;
                bool ActionsChanged = false;
                if (modlistData.ModsMustBeEnabled.Count > 0)
                {
                    foreach (var mod in modlistData.ModsMustBeEnabled)
                    {
                        if (!ActionsChanged)
                        {
                            ActionsChanged = true;
                            modlistData.Report.Add(T._("Results") + ":"
                            + Environment.NewLine
                            + Environment.NewLine
                            + T._("Actions") + ":"
                            + Environment.NewLine
                            );
                        }
                        modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was activated"));
                        ManageMO.ActivateMod(mod);
                        ListChanged = true;
                    }
                }

                if (modlistData.ModsMustBeDisabled.Count > 0)
                {
                    foreach (var mod in modlistData.ModsMustBeDisabled)
                    {
                        if (!ActionsChanged)
                        {
                            ActionsChanged = true;
                            modlistData.Report.Add(T._("Results") + ":"
                            + Environment.NewLine
                            + Environment.NewLine
                            + T._("Actions") + ":"
                            + Environment.NewLine
                            );
                        }
                        modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was deactivated"));
                        ManageMO.DeactivateMod(mod);
                        ListChanged = true;
                    }
                }

                //meta.ini fixes/tweaks
                //var reportCntPre = modlistData.Report.Count;
                OldMetaInfoFix(modlistData.Report);
                //var metainiFixesApplyed = modlistData.Report.Count > reportCntPre;

                if (modlistData.Report.Count > 0)
                {
                    var ReportMessage = string.Empty;
                    if (ListChanged)
                    {
                        ReportMessage += string.Join(Environment.NewLine, modlistData.Report);
                        //ReportMessage += T._("Results") + ":"
                        //    + Environment.NewLine
                        //    + Environment.NewLine
                        //    + string.Join(Environment.NewLine, modlistData.Report);
                    }
                    else
                    {
                        ReportMessage += T._("No known issues found in the enabled mods list")
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, modlistData.Report);
                    }

                    MessageBox.Show(ReportMessage);
                }
                else
                {
                    MessageBox.Show(T._("No known problems found"));
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

            /// <summary>
            /// Fix meta.ini game name value for mods
            /// </summary>
            internal static void OldMetaInfoFix(List<string> report)
            {
                var AllDirsList = Directory.GetDirectories(ManageSettings.GetCurrentGameModsPath());
                var ApplyModListgameNameValueFix = false;
                var cnt = 0;
                using (var INIFixesProgressBar = new ProgressBar())
                {
                    INIFixesProgressBar.Style = ProgressBarStyle.Blocks;
                    INIFixesProgressBar.Dock = DockStyle.Bottom;
                    INIFixesProgressBar.Height = 10;
                    INIFixesProgressBar.Maximum = AllDirsList.Length;

                    using (var INIFixesForm = new Form())
                    {
                        INIFixesForm.Text = T._("Meta info refreshing in progress") + "...";
                        INIFixesForm.Size = new System.Drawing.Size(370, 50);
                        bool metainiFixesApplyed = false;
                        foreach (var mod in AllDirsList)
                        {
                            cnt++;
                            if (!ApplyModListgameNameValueFix)
                            {
                                ApplyModListgameNameValueFix = true;

                                //show progress bar in new form
                                INIFixesForm.Controls.Add(INIFixesProgressBar);
                                INIFixesForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                                INIFixesForm.StartPosition = FormStartPosition.CenterScreen;
                                INIFixesForm.Show();
                                INIFixesForm.Activate();

                                //if (cnt == 10)
                                //{
                                //break;
                                //}
                            }

                            if (cnt < INIFixesProgressBar.Maximum)
                            {
                                INIFixesProgressBar.Value = cnt;
                            }

                            var modMetaIniPath = Path.Combine(mod, "meta.ini");
                            if (!File.Exists(modMetaIniPath))
                            {
                                continue;
                            }
                            var INI = new INIFile(modMetaIniPath);
                            var INIchanged = false;

                            //string gameName;
                            //fix for incorrect gameName set for the mod
                            if (INI.KeyExists("gameName", "General") && (/*gameName =*/ INI.ReadINI("General", "gameName")) != ManageSettings.GETMOCurrentGameName())
                            {
                                //if (!ApplyModListgameNameValueFix)
                                //{
                                //    var result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Found old mods meta gameName info") + "\n\n" + T._("Refresh it?"), T._("Confirmation"), MessageBoxButtons.YesNo);
                                //    if (result == DialogResult.Yes)
                                //    {
                                //        ApplyModListgameNameValueFix = true;

                                //        //show progress bar in new form
                                //        INIFixesForm.Controls.Add(INFixesProgressBar);
                                //        INIFixesForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                                //        INIFixesForm.StartPosition = FormStartPosition.CenterScreen;
                                //        INIFixesForm.Show();
                                //        INIFixesForm.Activate();
                                //    }
                                //    else
                                //    {
                                //        break;
                                //    }
                                //}

                                INI.WriteINI("General", "gameName", ManageSettings.GETMOCurrentGameName(), false);
                                if(!metainiFixesApplyed)
                                {
                                    metainiFixesApplyed = true;
                                    report.Add(Environment.NewLine+ "meta.ini fixes:");
                                }
                                report.Add(Path.GetFileName(mod)+": "+T._("fixed game name"));
                                INIchanged = true;

                            }

                            //Copy 1st found url from notes to url key
                            if (!INI.KeyExists("url", "General") || string.IsNullOrWhiteSpace(INI.ReadINI("General", "url")))
                            {
                                var metanotes = INI.ReadINI("General", "notes");
                                var regex = @"<a href\=\\""[^>]+\\"">";
                                var url = Regex.Match(metanotes, regex);
                                if (url.Success && !string.IsNullOrWhiteSpace(url.Value))
                                {
                                    var urlValue = url.Value.Remove(url.Value.Length - 3, 3).Remove(0, 10);
                                    INI.WriteINI("General", "url", urlValue, false);
                                    INI.WriteINI("General", "hasCustomURL", "true", false);
                                    if (!metainiFixesApplyed)
                                    {
                                        metainiFixesApplyed = true;
                                        report.Add(Environment.NewLine + "meta.ini fixes:");
                                    }
                                    report.Add(Path.GetFileName(mod) + ": " + T._("added url from notes"));
                                    INIchanged = true;
                                }
                            }
                            else if (INI.KeyExists("url", "General") && !string.IsNullOrWhiteSpace(INI.ReadINI("General", "url")))
                            {
                                if (INI.KeyExists("hasCustomURL", "General") || INI.ReadINI("General", "hasCustomURL") == "false")
                                {
                                    INI.WriteINI("General", "hasCustomURL", "true", false);
                                    INIchanged = true;
                                }
                            }

                            if (INIchanged)
                            {
                                INI.SaveINI();
                            }
                        }
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
