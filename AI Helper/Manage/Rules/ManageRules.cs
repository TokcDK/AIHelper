using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Rules.MetaIniFixes;
using AIHelper.Manage.Rules.ModList;

namespace AIHelper.Manage
{
    internal class ManageRules
    {
        internal class ModList
        {
            readonly ModListRulesData _modlistData;
            public ModList()
            {
                _modlistData = new ModListRulesData();
            }

            internal void ModlistFixes()
            {
                var modlistBackupFilePath = ManageModOrganizer.MakeMoProfileModlistFileBuckup("_prefixes");


                var modlist = new ModlistData();
                _modlistData.AllModsList = modlist.GetBy(ModlistData.ModType.ModAny).OrderByDescending(m => m.Priority).ToArray();
                _modlistData.EnabledModsListAndOverwrite = _modlistData.AllModsList.Where(m => m.IsEnabled).ToArray();
                var checkForm = new Form
                {
                    Size = new System.Drawing.Size(300, 50),
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = T._("Checking") + "..."
                };
                var checkProgress = new ProgressBar
                {
                    Dock = DockStyle.Fill,
                    Maximum = _modlistData.EnabledModsListAndOverwrite.Length
                };
                var cnt = 0;
                checkForm.Controls.Add(checkProgress);
                checkForm.Show();

                foreach (var mod in modlist.GetBy(ModlistData.ModType.ModEnabled))
                {
                    if (cnt < checkProgress.Maximum) checkProgress.Value = cnt;

                    _modlistData.Mod = mod;
                    ApplyRules();

                    cnt++;
                }

                checkProgress.Dispose();
                checkForm.Dispose();

                KPlugTweaks();

                _modlistData.Report = _modlistData.AllModsList.Where(m => m.ReportMessages.Count > 0).Select(m => $"{m.Name}: {string.Join($"\n{m.Name}: ", m.ReportMessages)}").ToList();

                var listChanged = _modlistData.Report.Count > 0;
                //if (listChanged) modlist.Save();

                //bool actionsChanged = false;
                //if (_ModlistData.Mod.NamesMustBeEnabled.Count > 0)
                //{
                //    foreach (var mod in _ModlistData.Mod.NamesMustBeEnabled)
                //    {
                //        if (!actionsChanged)
                //        {
                //            actionsChanged = true;
                //            _modlistData.Report.Add(T._("Results") + ":"
                //            + Environment.NewLine
                //            + Environment.NewLine
                //            + T._("Actions") + ":"
                //            + Environment.NewLine
                //            );
                //        }
                //        _modlistData.Report.Add(T._("Mod") + " " + mod.Key + " " + T._("was activated") + ": " + mod.Value);
                //        ManageModOrganizer.ActivateMod(mod.Key);
                //        listChanged = true;
                //    }
                //}

                //if (_ModlistData.Mod.NamesMustBeDisabled.Count > 0)
                //{
                //    foreach (var modinfo in _ModlistData.Mod.NamesMustBeDisabled)
                //    {
                //        var mod = modinfo.Key;
                //        if (!actionsChanged)
                //        {
                //            actionsChanged = true;
                //            _modlistData.Report.Add(T._("Results") + ": "
                //            + Environment.NewLine
                //            + Environment.NewLine
                //            + T._("Actions") + ":"
                //            + Environment.NewLine
                //            );
                //        }
                //        _modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was deactivated") + ": " + modinfo.Value);
                //        ManageModOrganizer.DeactivateMod(mod);
                //        listChanged = true;
                //    }
                //}

                //meta.ini fixes/tweaks
                //var reportCntPre = modlistData.Report.Count;
                MetaInfoFixes();
                //var metainiFixesApplyed = modlistData.Report.Count > reportCntPre;

                if (_modlistData.Report.Count > 0)
                {
                    var reportMessage = string.Empty;
                    if (listChanged)
                    {
                        reportMessage += string.Join(Environment.NewLine, _modlistData.Report);
                        //ReportMessage += T._("Results") + ":"
                        //    + Environment.NewLine
                        //    + Environment.NewLine
                        //    + string.Join(Environment.NewLine, modlistData.Report);
                    }
                    else
                    {
                        reportMessage += T._("No known issues found in the enabled mods list")
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, _modlistData.Report);
                    }

                    var reportForm = new Form
                    {
                        Text = T._("Modlist check report"),
                        //ReportForm.Size = new System.Drawing.Size(500,700);
                        AutoSize = true,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    var reportTb = new RichTextBox
                    {
                        Size = new System.Drawing.Size(700, 900),
                        WordWrap = true,
                        Dock = DockStyle.Fill,
                        ReadOnly = true,
                        //ReportTB.BackColor = System.Drawing.Color.Gray;
                        Text = reportMessage,
                        ScrollBars = RichTextBoxScrollBars.Both
                    };

                    reportForm.Controls.Add(reportTb);
                    reportForm.Show();

                    //MessageBox.Show(ReportMessage);
                }
                else
                {
                    MessageBox.Show(T._("No known problems found"));
                }

                if (!listChanged && !string.IsNullOrWhiteSpace(modlistBackupFilePath))
                {
                    try
                    {
                        File.Delete(modlistBackupFilePath);
                    }
                    catch
                    {
                    }
                }
            }

            /// <summary>
            /// apply tweak for kplug
            /// </summary>
            private void KPlugTweaks()
            {
                var cfgpath = Path.Combine(ManageSettings.CurrentGameMoOverwritePath, "BepInEx", "config", "KK_Fix_MainGameOptimizations.cfg");

                if (!File.Exists(cfgpath))
                {
                    foreach (var mod in _modlistData.EnabledModsListAndOverwrite)
                    {
                        if (File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, mod.Name, "BepInEx", "config", "KK_Fix_MainGameOptimizations.cfg")))
                        {
                            cfgpath = Path.Combine(ManageSettings.CurrentGameModsDirPath, mod.Name, "BepInEx", "config", "KK_Fix_MainGameOptimizations.cfg");
                            break;
                        }
                    }
                }

                if (File.Exists(cfgpath))
                {
                    var value = ManageCfg.GetCfgValueIfExist(cfgpath, "Async clothes loading", "Tweaks");
                    //set Async clothes loading to false in kplug enabled
                    if (!string.IsNullOrWhiteSpace(value) && ((_modlistData.KPlugEnabled && value.Length == 4) || (!_modlistData.KPlugEnabled && value.Length == 5)))
                    {
                        ManageCfg.WriteCfgValue(cfgpath, "Tweaks", "Async clothes loading", (!_modlistData.KPlugEnabled).ToString());

                        if (_modlistData.KPlugEnabled)
                        {
                            _modlistData.Report.Add(T._("kPlug tweaks:"));
                            _modlistData.Report.Add(T._("KK_Fix_MainGameOptimizations 'Async clothes loading' parameter was set to 'False' to prevent problems with kPlug"));
                        }
                    }
                }
            }

            /// <summary>
            /// Fix meta.ini issues
            /// </summary>
            internal void MetaInfoFixes()
            {
                ModlistDataMetaIniFixesTools.ApplyFixes(_modlistData);
            }

            private void ApplyRules()
            {
                foreach (var rule in _modlistData.RulesList)
                {
                    if (!rule.Condition()) continue;

                    //var oldMustEnabledCount = _ModlistData.Mod.NamesMustBeEnabled.Count;
                    //var oldMustEnabledCandidatesCount = _ModlistData.Mod.NamesMustBeEnabledCandidates.Count;
                    //var oldMustDisaledCount = _ModlistData.Mod.NamesMustBeDisabled.Count;
                    //var oldMustDisaledCandidatesCount = _ModlistData.Mod.NamesMustBeDisabledCandidates.Count;

                    if (!rule.Fix()) continue; // ClearCandidates();

                    //if (!_ModlistData.Mod.NamesMustBeDisabledCandidates.ContainsKey(_ModlistData.Mod.Name) && !_ModlistData.Mod.NamesMustBeDisabled.ContainsKey(_ModlistData.Mod.Name))
                    //{
                    //    AddCandidates();
                    //}
                    //else
                    //{
                    //    _ModlistData.Mod.NamesMustBeEnabledCandidates.Clear();
                    //    AddCandidates();
                    //}
                    //if (oldMustEnabledCount != _ModlistData.Mod.NamesMustBeEnabled.Count
                    //    ||
                    //    oldMustEnabledCandidatesCount != _ModlistData.Mod.NamesMustBeEnabledCandidates.Count
                    //    ||
                    //    oldMustDisaledCount != _ModlistData.Mod.NamesMustBeDisabled.Count
                    //    ||
                    //    oldMustDisaledCandidatesCount != _ModlistData.Mod.NamesMustBeDisabledCandidates.Count
                    //    )
                    //{
                    //    var report = T._("For mod") + " \"" + _ModlistData.Mod.Name + "\" " + T._("was applied rule") + " \"" + rule.Description() + "\"" + (rule.Result.Length > 0 ? (" " + T._("with result") + ":" + " \" " + rule.Result) : rule.Result);
                    //    if (!_modlistData.Report.Contains(report)) _modlistData.Report.Add(report);
                    //}
                }
            }

            private void ClearCandidates()
            {
                //_ModlistData.Mod.NamesMustBeEnabledCandidates.Clear();
                //_ModlistData.Mod.NamesMustBeDisabledCandidates.Clear();
            }

            private void AddCandidates()
            {
                //AddCandidatesToMain(_ModlistData.Mod.NamesMustBeEnabledCandidates, _ModlistData.Mod.NamesMustBeEnabled);
                //AddCandidatesToMain(_ModlistData.Mod.NamesMustBeDisabledCandidates, _ModlistData.Mod.NamesMustBeDisabled);
                //ClearCandidates();
            }

            private static void AddCandidatesToMain(Dictionary<string, string> candidates, Dictionary<string, string> parent)
            {
                //if (candidates.Count > 0)
                //{
                //    foreach (var candidate in candidates)
                //    {
                //        if (!parent.ContainsKey(candidate.Key))
                //        {
                //            parent.Add(candidate.Key, candidate.Value);
                //        }
                //    }
                //}
            }
        }
    }
}
