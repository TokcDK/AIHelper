using AIHelper.Manage;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Dialog with the options the mod update pass runs with.
    /// </summary>
    public partial class UpdateOptionsDialogForm : Form
    {
        public UpdateOptionsDialogForm()
        {
            InitializeComponent();
        }

        private void OpenOldVersionsDirButton_Click(object sender, EventArgs e)
        {
            var modUpdatesBakDir = ManageSettings.UpdatedModsOlderVersionsBuckupDirPath;
            if (!Directory.Exists(modUpdatesBakDir))
            {
                Directory.CreateDirectory(modUpdatesBakDir);
            }
            Process.Start("explorer.exe", modUpdatesBakDir);
        }

        private void UpdateZipmodsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateButtonOptionsRefresh();
        }

        private void BleadingEdgeZipmodsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (BleadingEdgeZipmodsCheckBox.Checked && !File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ManageSettings.ZipmodsBleedingEdgeMarkFilePath));
                File.WriteAllText(ManageSettings.ZipmodsBleedingEdgeMarkFilePath, string.Empty);
            }
            else if (!BleadingEdgeZipmodsCheckBox.Checked && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath))
            {
                File.Delete(ManageSettings.ZipmodsBleedingEdgeMarkFilePath);
            }

            UpdateButtonOptionsRefresh();
        }

        private void UpdatePluginsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateButtonOptionsRefresh();
        }

        private void CheckEnabledModsOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateButtonOptionsRefresh();
        }

        /// <summary>
        /// update status of update button options and button itself
        /// </summary>
        private void UpdateButtonOptionsRefresh()
        {
            UpdateZipmodsCheckBox.Enabled = ManageSettings.IsHaveSideloaderMods&& File.Exists(ManageSettings.KkManagerStandaloneUpdaterExePath);
            if (UpdateZipmodsCheckBox.Checked && !UpdateZipmodsCheckBox.Enabled)
            {
                UpdateZipmodsCheckBox.Checked = false;
            }
            BleadingEdgeZipmodsCheckBox.Enabled = UpdateZipmodsCheckBox.Enabled;
            if (!UpdateZipmodsCheckBox.Enabled)
            {
                UpdateOptionsFlowLayoutPanel.Controls.Remove(UpdateZipmodsCheckBox);
                UpdateOptionsFlowLayoutPanel.Controls.Remove(BleadingEdgeZipmodsCheckBox);
            }

            CheckEnabledModsOnlyCheckBox.Enabled = UpdatePluginsCheckBox.Checked;
            //ManageSettings.MainForm.CheckEnabledModsOnlyLabel.SetCheck(CheckEnabledModsOnlyCheckBox.Enabled);
            btnUpdateMods.Enabled = (UpdatePluginsCheckBox.Visible && UpdatePluginsCheckBox.Checked) || (UpdateZipmodsCheckBox.Visible && UpdateZipmodsCheckBox.Checked);

            BleadingEdgeZipmodsCheckBox.Checked = BleadingEdgeZipmodsCheckBox.Enabled && UpdateZipmodsCheckBox.Checked && File.Exists(ManageSettings.ZipmodsBleedingEdgeMarkFilePath);
            BleadingEdgeZipmodsCheckBox.Enabled = BleadingEdgeZipmodsCheckBox.Enabled && UpdateZipmodsCheckBox.Checked;
        }

        private void UpdateOptionsDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the dialog is opened from the tools tab while the program runs, so it
            // takes the theme in use rather than the one which was current when it was
            // created
            ThemesLoader.ApplyTheme(this);

            SetTooltips();

            UpdateButtonOptionsRefresh();

            this.Location = ManageSettings.MainForm.Location;
            this.Size = ManageSettings.MainForm.Size;
        }

        private void SetTranslations()
        {
            this.Text = T._("Update options");
            UpdateZipmodsCheckBox.Text = T._("Update zipmods");
            BleadingEdgeZipmodsCheckBox.Text = T._("Check test zipmods pack");
            UpdatePluginsCheckBox.Text = T._("Update plugins");
            CheckEnabledModsOnlyCheckBox.Text = T._("Only enabled plugins");
            OpenOldVersionsDirButton.Text = T._("Old versions");
            btnUpdateMods.Text = T._("Start");
            btnCancel.Text = T._("Cancel");
        }

        private UiTooltip _toolTip;

        /// <summary>
        /// remove all tooltips and dispose resources
        /// </summary>
        internal void DisposeTooltips()
        {
            if (_toolTip == null) return;

            _toolTip.Dispose();
            _toolTip = null;
        }

        internal void SetTooltips()
        {
            DisposeTooltips();

            _toolTip = new UiTooltip();

            var sideloaderPacksWarning = T._("Warning! More of packs you check more of memory game will consume.") + "\n" +
                T._("Check only what you really using or you can 16+ gb of memory.");

            _toolTip.Set(OpenOldVersionsDirButton, T._("Open older plugins buckup folder"));
            _toolTip.Set(btnUpdateMods,
                T._("Update Mod Organizer and enabled mods") + "\n" +
                T._("Mod Organizer already have hardcoded info") + "\n" +
                T._("Mods will be updated if there exist info in meta.ini notes or in updateInfo.txt") + "\n" +
                T._("After plugins update check will be executed KKManager StandaloneUpdater for Sideloader modpack updates check for games where it is possible"));
            _toolTip.Set(UpdateZipmodsCheckBox,
                T._("Check if need to run update check for sideloader modpacks.") + "\n\n" +
                sideloaderPacksWarning);
            _toolTip.Set(UpdatePluginsCheckBox, T._("Check if need to run update check for plugins and Mod Organizer."));
            _toolTip.Set(CheckEnabledModsOnlyCheckBox, T._("Check updates only for enabled plugins."));
            _toolTip.Set(BleadingEdgeZipmodsCheckBox,
                T._("Check also updates of Bleeding Edge Sideloader Modpack in KKManager") + "\n" +
                T._("Bleeding Edge Sideloader modpack contains test versions of zipmods which is still not added in main modpacks") + "\n\n" +
                sideloaderPacksWarning);
        }

        private void UpdateOptionsDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeTooltips();
        }
    }
}
