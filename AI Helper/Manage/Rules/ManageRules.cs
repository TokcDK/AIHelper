using AIHelper.Manage.Rules.MetaIniFixes;
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
            readonly ModListData _modlistData;
            public ModList()
            {
                _modlistData = new ModListData();
            }

            internal void ModlistFixes()
            {
                var modlistBackupFilePath = ManageModOrganizer.MakeMoProfileModlistFileBuckup("_prefixes");

                var checkForm = new Form
                {
                    Size = new System.Drawing.Size(300, 50),
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = T._("Checking") + "..."
                };
                //var checkProgress = new ProgressBar
                //{
                //    Dock = DockStyle.Fill,
                //    Maximum = _modlistData.EnabledModNamesList.Length
                //};
                var cnt = 0;
                //checkForm.Controls.Add(checkProgress);
                checkForm.Show();

                _modlistData.AllModNamesList = ManageModOrganizer.GetModNamesListFromActiveMoProfile(false);
                _modlistData.EnabledModNamesList = ManageModOrganizer.GetModNamesListFromActiveMoProfile();
                foreach (var modName in _modlistData.EnabledModNamesList)
                {
                    if (string.IsNullOrWhiteSpace(modName)) continue;

                    //if (cnt < checkProgress.Maximum) checkProgress.Value = cnt;

                    _modlistData.ModName = modName;
                    ApplyRules();

                    cnt++;
                }

                //checkProgress.Dispose();
                checkForm.Dispose();

                KPlugTweaks();

                var listChanged = _modlistData.Report.Count > 0;
                bool actionsChanged = false;
                if (_modlistData.ModsMustBeEnabled.Count > 0)
                {
                    foreach (var mod in _modlistData.ModsMustBeEnabled)
                    {
                        if (!actionsChanged)
                        {
                            actionsChanged = true;
                            _modlistData.Report.Add(T._("Results") + ":"
                            + Environment.NewLine
                            + Environment.NewLine
                            + T._("Actions") + ":"
                            + Environment.NewLine
                            );
                        }
                        _modlistData.Report.Add(T._("Mod") + " " + mod.Key + " " + T._("was activated") + ": " + mod.Value);
                        ManageModOrganizer.ActivateMod(mod.Key);
                        listChanged = true;
                    }
                }

                if (_modlistData.ModsMustBeDisabled.Count > 0)
                {
                    foreach (var modinfo in _modlistData.ModsMustBeDisabled)
                    {
                        var mod = modinfo.Key;
                        if (!actionsChanged)
                        {
                            actionsChanged = true;
                            _modlistData.Report.Add(T._("Results") + ": "
                            + Environment.NewLine
                            + Environment.NewLine
                            + T._("Actions") + ":"
                            + Environment.NewLine
                            );
                        }
                        _modlistData.Report.Add(T._("Mod") + " " + mod + " " + T._("was deactivated") + ": " + modinfo.Value);
                        ManageModOrganizer.DeactivateMod(mod);
                        listChanged = true;
                    }
                }

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
                    foreach (var modName in _modlistData.EnabledModNamesList)
                    {
                        if (File.Exists(Path.Combine(ManageSettings.CurrentGameModsDirPath, modName, "BepInEx", "config", "KK_Fix_MainGameOptimizations.cfg")))
                        {
                            cfgpath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName, "BepInEx", "config", "KK_Fix_MainGameOptimizations.cfg");
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
                    if (rule.Condition())
                    {
                        var oldMustEnabledCount = _modlistData.ModsMustBeEnabled.Count;
                        var oldMustEnabledCandidatesCount = _modlistData.ModsMustBeEnabledCandidates.Count;
                        var oldMustDisaledCount = _modlistData.ModsMustBeDisabled.Count;
                        var oldMustDisaledCandidatesCount = _modlistData.ModsMustBeDisabledCandidates.Count;

                        if (rule.Fix())
                        {
                            if (!_modlistData.ModsMustBeDisabledCandidates.ContainsKey(_modlistData.ModName) && !_modlistData.ModsMustBeDisabled.ContainsKey(_modlistData.ModName))
                            {
                                AddCandidates();
                            }
                            else
                            {
                                _modlistData.ModsMustBeEnabledCandidates.Clear();
                                AddCandidates();
                            }
                            if (oldMustEnabledCount != _modlistData.ModsMustBeEnabled.Count
                                ||
                                oldMustEnabledCandidatesCount != _modlistData.ModsMustBeEnabledCandidates.Count
                                ||
                                oldMustDisaledCount != _modlistData.ModsMustBeDisabled.Count
                                ||
                                oldMustDisaledCandidatesCount != _modlistData.ModsMustBeDisabledCandidates.Count
                                )
                            {
                                var report = T._("For mod") + " \"" + _modlistData.ModName + "\" " + T._("was applied rule") + " \"" + rule.Description() + "\"" + (rule.Result.Length > 0 ? (" " + T._("with result") + ":" + " \" " + rule.Result) : rule.Result);
                                if (!_modlistData.Report.Contains(report))
                                {
                                    _modlistData.Report.Add(report);
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
                _modlistData.ModsMustBeEnabledCandidates.Clear();
                _modlistData.ModsMustBeDisabledCandidates.Clear();
            }

            private void AddCandidates()
            {
                AddCandidatesToMain(_modlistData.ModsMustBeEnabledCandidates, _modlistData.ModsMustBeEnabled);
                AddCandidatesToMain(_modlistData.ModsMustBeDisabledCandidates, _modlistData.ModsMustBeDisabled);
                ClearCandidates();
            }

            private static void AddCandidatesToMain(Dictionary<string, string> candidates, Dictionary<string, string> parent)
            {
                if (candidates.Count > 0)
                {
                    foreach (var candidate in candidates)
                    {
                        if (!parent.ContainsKey(candidate.Key))
                        {
                            parent.Add(candidate.Key, candidate.Value);
                        }
                    }
                }
            }
        }
    }
}
