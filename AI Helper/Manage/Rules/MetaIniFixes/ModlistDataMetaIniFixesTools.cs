using AIHelper.Manage.Rules.ModList;
using GetListOfSubClasses;
using System;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class ModlistDataMetaIniFixesTools
    {
        internal static void ApplyFixes(ModListData modlistData)
        {
            modlistData.Report.Add(Environment.NewLine + "meta.ini fixes:");

            var allDirsList = Directory.GetDirectories(ManageSettings.GetCurrentGameModsDirPath());
            var cnt = 0;
            using (var iniFixesProgressBar = new ProgressBar())
            {
                iniFixesProgressBar.Style = ProgressBarStyle.Blocks;
                iniFixesProgressBar.Dock = DockStyle.Bottom;
                iniFixesProgressBar.Height = 10;
                iniFixesProgressBar.Maximum = allDirsList.Length;
                var iniFixesProgressBarMaximum = iniFixesProgressBar.Maximum;

                using (var iniFixesForm = new Form())
                {
                    iniFixesForm.Text = T._("Meta info refreshing in progress") + "...";
                    iniFixesForm.Size = new System.Drawing.Size(370, 50);

                    //show progress bar in new form
                    iniFixesForm.Controls.Add(iniFixesProgressBar);
                    iniFixesForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                    iniFixesForm.StartPosition = FormStartPosition.CenterScreen;
                    iniFixesForm.Show();
                    iniFixesForm.Activate();

                    bool inIchanged = false;
                    var preModlistCount = modlistData.Report.Count;
                    foreach (var mod in allDirsList)
                    {
                        cnt++;

                        if (cnt < iniFixesProgressBarMaximum)
                        {
                            iniFixesProgressBar.Value = cnt;
                        }

                        var modMetaIniPath = Path.Combine(mod, "meta.ini");
                        if (!File.Exists(modMetaIniPath))
                        {
                            continue;
                        }
                        var ini = ManageIni.GetINIFile(modMetaIniPath);

                        foreach (var fix in Inherited.GetInheritedSubClasses<ModlistDataMetaIniFixesBase>(modlistData, ini, mod))
                        {
                            if (fix.Apply() && !inIchanged)
                            {
                                inIchanged = true;
                            }
                        }

                        if (inIchanged)
                        {
                            ini.WriteFile();
                        }
                    }

                    if (preModlistCount == modlistData.Report.Count && modlistData.Report.Count > 0)
                    {
                        // remove fixes title if was no fixes
                        modlistData.Report.RemoveAt(modlistData.Report.Count - 1);
                    }
                }
            }
        }
    }
}
