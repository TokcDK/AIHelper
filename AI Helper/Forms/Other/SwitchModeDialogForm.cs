using AIHelper.Manage;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Dialog which explains what a mode switch does and asks whether to go ahead.
    /// </summary>
    public partial class SwitchModeDialogForm : Form
    {
        public SwitchModeDialogForm()
        {
            InitializeComponent();
        }

        private void SwitchModeDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the dialog is opened from the tools tab while the program runs, so it
            // takes the theme in use rather than the one which was current when it was
            // created
            ThemesLoader.ApplyTheme(this);

            SetTooltips();

            if (!ManageSettings.IsMoMode)
            {
                MakeBuckupCheckBox.Checked = false;
                MakeBuckupCheckBox.Visible = false;
                DoNotSwitchCheckBox.Visible = MakeBuckupCheckBox.Visible;
            }

            this.Location = ManageSettings.MainForm.Location;
            this.Size = ManageSettings.MainForm.Size;
        }

        private void SetTranslations()
        {
            MessageTextTextBox.Text = (ManageSettings.IsMoMode? T._(
                    "Game mode will be changed to normal when " +
                    "used files will be moved to Data dir.\n" +
                    "Game will be executing raw from its exe.\n" +
                    "If checked Make backup then will be made\n" +
                    "a file structure backup of Data and Mods dirs\n" +
                    "using ntfs hard links not consuming disc space."
                ) : T._(
                    "Game mode will be changed back to MO when " +
                    "used files will be moved from Data to their\n" +
                    "mods original locations and new files will\n" +
                    "be moved in added mod so you can use MO features.\n" +
                    "Game will be executing from Mod Organizer."
                )).Replace("\n", "\r\n");

            this.Text = T._("Mode switch. Current mode:") + " " + (ManageSettings.IsMoMode? T._("MO mode") : T._("Normal mode"));
            MakeBuckupCheckBox.Text = T._("Make backup");
            btnSwitchMode.Text = T._("Switch mode");
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

            _toolTip.Set(MakeBuckupCheckBox,
                T._("Enables backup creation of selected game before mode switch\n" +
                " to be possible to restore Data and Mods dirs.\n" +
                "\n" +
                "Backup creating using ntfs hardlinks and not consumes any extra space."));
        }
    }
}
