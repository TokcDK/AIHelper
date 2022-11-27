using AIHelper.Manage.Rules.ModList;
using GetListOfSubClasses;
using System;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Manage.Rules.MetaIniFixes
{
    class ModlistDataMetaIniFixesTools
    {
        internal static void ApplyFixes(ModListRulesData modlistData)
        {
            modlistData.Report.Add(Environment.NewLine + "meta.ini fixes:");

            var allDirsList = Directory.GetDirectories(ManageSettings.CurrentGameModsDirPath);
            var cnt = 0;
            var iniFixesProgressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Blocks,
                Dock = DockStyle.Bottom,
                Height = 10,
                Maximum = allDirsList.Length
            };
            var iniFixesProgressBarMaximum = iniFixesProgressBar.Maximum;

            var iniFixesForm = new Form
            {
                Text = T._("Meta info refreshing in progress") + "...",
                Size = new System.Drawing.Size(370, 50),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition = FormStartPosition.CenterScreen
            };

            //show progress bar in new form
            iniFixesForm.Controls.Add(iniFixesProgressBar);
            iniFixesForm.Show();
            iniFixesForm.Activate();

            bool inIchanged = false;
            var preModlistCount = modlistData.Report.Count;
            foreach (var mod in allDirsList)
            {
                cnt++;

                if (cnt < iniFixesProgressBarMaximum) iniFixesProgressBar.Value = cnt;

                var modMetaIniPath = Path.Combine(mod, "meta.ini");
                if (!File.Exists(modMetaIniPath)) continue;

                var ini = ManageIni.GetINIFile(modMetaIniPath);

                foreach (var fix in Inherited.GetInheritedSubClasses<ModlistDataMetaIniFixesBase>(modlistData, ini, mod))
                {
                    if (fix.Apply() && !inIchanged) inIchanged = true;
                }

                if (inIchanged) ini.WriteFile();
            }

            // remove fixes title if was no fixes
            if (preModlistCount == modlistData.Report.Count && modlistData.Report.Count > 0)
            {
                modlistData.Report.RemoveAt(modlistData.Report.Count - 1);
            }

            iniFixesProgressBar.Dispose();
            iniFixesForm.Dispose();
        }
    }
}
