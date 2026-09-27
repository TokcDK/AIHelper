using AIHelper.Manage;
using AIHelper.Manage.ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements.BepinEx
{
    internal partial class BepinExForm : Form
    {
        public BepinExForm()
        {
            InitializeComponent();

            //the tool tip is not part of the designer's component container, so it is
            //released when the panel is
            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            DisposeTooltips();
        }

        double _version;
        string _cfgPath;
        private void BepinExForm_Load(object sender, EventArgs e)
        {
            _version = ManageStringsExtensions.GetProductVersionToFloatNumber(GetBepInExVersionString());
            SetLocalization();
            InitSettings();
            SetTooltips();
        }

        private UiTooltip _toolTip;

        private void SetTooltips()
        {
            DisposeTooltips();

            _toolTip = new UiTooltip();

            _toolTip.Set(BepinExLogTargetLinkLabel, T._("Log target selection.\n\n" +
                "Show logs in console window or to file on disk.\nOther log parameters depend on selected target"));
            _toolTip.Set(BepInExLevelsSelectAllLabel, T._("Select all log levels"));
            _toolTip.Set(BepInExLevelsClearLabel, T._("Clear selection of log levels"));
            _toolTip.Set(BepInExSettingsLogCheckedListBox, T._("Log levels. How many messages will be displayed"));

            _toolTip.Set(OpenLogLinkLabel, T._("Open BepinEx log if found"));
            _toolTip.Set(BepInExSettingsDisplayedLogLevelLabel, T._("Click here to select log level\n" +
                "Only displays the specified log level and above in the console output"));
            _toolTip.Set(BepInExSettingsLogCheckBox, T._("Click to enable log"));

            _toolTip.Set(BepinExHelpLinkLabel, T._("Open BepInEx documentation"));
            _toolTip.Set(BepinExLogOpenConfigFileLinkLabel, T._("Open BepInEx configuration file in Notepad"));
        }

        private void DisposeTooltips()
        {
            if (_toolTip == null) return;

            _toolTip.Dispose();
            _toolTip = null;
        }

        private string GetBepInExVersionString()
        {
            _cfgPath = ManageSettings.BepInExCfgFilePath;
            var fullPath = Path.GetFullPath(Path.Combine(_cfgPath, "..", "..", "core", "BepInEx.dll"));
            var bepInExDllPath = ManageModOrganizer.GetLastPath(fullPath);
            if (File.Exists(bepInExDllPath))
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(bepInExDllPath);
                var ver = fileVersionInfo.FileVersion;
                return ver;
            }

            return ManageSettings.DefaultBepInEx5OlderVersion;
        }

        bool _isInit;
        private void InitSettings()
        {
            _isInit = true;
            InitLogSettings();
            _isInit = false;
        }

        private void InitLogSettings()
        {
            //Set BepInEx log data
            if (ManageSettings.BepInExCfgFilePath.Length > 0 && File.Exists(ManageSettings.BepInExCfgFilePath))
            {
                BepInExSettingsLogCheckBox.Enabled = true;

                var logTargetSection = GetDiscConsoleSectionName();

                try
                {
                    BepInExSettingsLogCheckBox.Checked = bool.Parse(ManageCfg.GetCfgValueIfExist(ManageSettings.BepInExCfgFilePath, "Enabled", logTargetSection, "False"));
                }
                catch
                {
                    BepInExSettingsLogCheckBox.Checked = false;
                }

                if (BepInExSettingsDisplayedLogLevelLabel.Visible = BepInExSettingsLogCheckBox.Checked)
                {
                    var logLevels = ManageCfg.GetCfgValueIfExist(_cfgPath, "LogLevels", logTargetSection);

                    ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExSettingsLogCheckBox, BepInExSettingsDisplayedLogLevelLabel, true, logTargetSection);

                    var levels = logLevels.Replace(", ", ",").Split(',');

                    CheckLevels(levels);
                }
            }
        }

        private string GetDiscConsoleSectionName()
        {
            var logTargetSection = "Logging.Disk";
            if (BepinExLogTargetLinkLabel.Text == T._("Console"))
            {
                logTargetSection = "Logging.Console";
            }
            return logTargetSection;
        }

        private void SetLocalization()
        {
            BepinExLogTargetLinkLabel.Text = T._("Console");
            BepinExLogOpenConfigFileLinkLabel.Text = T._("Settings");
            BepInExLogLevelsSourceLabel.Text = T._("target") + ":";
            BepInExLogLevelsLabel.Text = T._("levels") + ":";
        }

        private void WriteLevels()
        {
            ////https://stackoverflow.com/questions/3666682/which-checkedlistbox-event-triggers-after-a-item-is-checked
            //List<string> checkedItems = new List<string>();
            //foreach (var item in BepInExSettingsLogCheckedListBox.CheckedItems)
            //    checkedItems.Add(item.ToString());

            //if (e.NewValue == CheckState.Checked)
            //    checkedItems.Add(BepInExSettingsLogCheckedListBox.Items[e.Index].ToString());
            //else
            //    checkedItems.Remove(BepInExSettingsLogCheckedListBox.Items[e.Index].ToString());

            var logTargetSection = GetDiscConsoleSectionName();
            var levelsValue = "None";
            if (BepInExSettingsLogCheckedListBox.CheckedItems.Count == 0)
            {
            }
            else
            {
                if (BepInExSettingsLogCheckedListBox.CheckedItems.Count == BepInExSettingsLogCheckedListBox.Items.Count)
                {
                    levelsValue = "All";
                }
                else
                {
                    var levels = new List<string>();
                    foreach (var item in BepInExSettingsLogCheckedListBox.CheckedItems)
                    {
                        levels.Add(item.ToString());
                    }
                    //if (levels.Count > 0)
                    {
                        levelsValue = string.Join(", ", levels.ToArray());
                    }
                }
            }
            //MessageBox.Show("LevelsValue=" + Environment.NewLine + LevelsValue);
            ManageCfg.WriteCfgValue(_cfgPath, logTargetSection, "LogLevels", levelsValue);
        }

        private void CheckLevels(string[] levels)
        {
            var ind = 0;
            var checks = new bool[BepInExSettingsLogCheckedListBox.Items.Count];
            foreach (var item in BepInExSettingsLogCheckedListBox.Items)
            {
                var b = levels.Contains(item.ToString());
                checks[ind] = b;
                ind++;
            }
            ind = 0;
            foreach (var item in checks)
            {
                BepInExSettingsLogCheckedListBox.SetItemChecked(ind, checks[ind]);
                ind++;
            }
        }

        private void BepInExSettingsLogCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isInit)
                return;

            //None
            //Fatal d
            //Error d
            //Warning
            //Message d
            //Info d
            //Debug
            //All

            //Delayed execution solution for ItemCheck event:https://stackoverflow.com/a/16936590
            //this.BeginInvoke((MethodInvoker)(
            //    () => 
            //    WriteLevels()
            //    ));

            //manual check solution for ItemCheck event: https://stackoverflow.com/a/17511730
            var clb = (CheckedListBox)sender;
            // Switch off event handler
            clb.ItemCheck -= BepInExSettingsLogCheckedListBox_ItemCheck;

            clb.SetItemCheckState(e.Index, e.NewValue);

            // Switch on event handler
            clb.ItemCheck += BepInExSettingsLogCheckedListBox_ItemCheck;

            WriteLevels();
        }

        private void ClbCheckAllItems(CheckedListBox clb = null)
        {
            if (clb == null)
                clb = BepInExSettingsLogCheckedListBox;

            var cnt = clb.Items.Count;
            for (int i = 0; i < cnt; i++)
            {
                clb.SetItemCheckState(i, CheckState.Checked);
            }
        }

        private void ClbUncheckAllItems(CheckedListBox clb = null)
        {
            if (clb == null)
                clb = BepInExSettingsLogCheckedListBox;

            foreach (int i in clb.CheckedIndices)
            {
                clb.SetItemCheckState(i, CheckState.Unchecked);
            }
        }

        private void BepinExLogOpenConfigFileLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(_cfgPath))
            {
                Process.Start("notepad.exe", _cfgPath);
            }
        }

        private void BepinExLogTargetLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _isInit = true;
            if ((sender as LinkLabel).Text == T._("Disk"))
            {
                (sender as LinkLabel).Text = T._("Console");
            }
            else
            {
                (sender as LinkLabel).Text = T._("Disk");
            }
            InitLogSettings();
            _isInit = false;
        }

        private void BepinExHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/BepInEx/BepInEx/wiki");
        }

        private void BepInExLevelsSelectAllLabel_Click(object sender, EventArgs e)
        {
            _isInit = true;
            ClbCheckAllItems();
            WriteLevels();
            _isInit = false;
        }

        private void BepInExLevelsClearLabel_Click(object sender, EventArgs e)
        {
            _isInit = true;
            ClbUncheckAllItems();
            WriteLevels();
            _isInit = false;
        }

        private void BepInExSettingsLogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInit)
                return;

            ManageCfg.WriteCfgValue(_cfgPath, "Logging.Console", "Enabled", /*" " +*/ (sender as CheckBox).Checked.ToString(CultureInfo.InvariantCulture));
        }

        private void OpenLogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManageModOrganizer.OpenBepinexLog();
        }

        private void BepInExSettingsDisplayedLogLevelLabel_Click(object sender, EventArgs e)
        {
            if (BepInExSettingsLogCheckBox.Checked)
            {
                ManageSettings.SwitchBepInExDisplayedLogLevelValue(BepInExSettingsLogCheckBox, BepInExSettingsDisplayedLogLevelLabel, false, GetDiscConsoleSectionName());
            }
        }
    }
}
