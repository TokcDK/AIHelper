using AIHelper.Manage;
using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    public partial class SwitchModeDialogForm : Form
    {
        public SwitchModeDialogForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SwitchModeDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();
            SetTooltips();

            if (!ManageSettings.IsMoMode)
            {
                MakeBuckupCheckBox.Checked = false;
                MakeBuckupCheckBox.Visible = false;
            }
        }

        private ToolTip _thToolTip;
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
            button1.Text = T._("Switch mode");
            button2.Text = T._("Cancel");
        }

        /// <summary>
        /// remove all tooltips and dispose resources
        /// </summary>
        internal void DisposeTooltips()
        {
            try
            {
                if (_thToolTip != null)
                {
                    _thToolTip.RemoveAll();
                    _thToolTip.Dispose();
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured while SetTooltips. error:\r\n" + ex);
            }
        }

        internal void SetTooltips()
        {
            DisposeTooltips();

            //http://qaru.site/questions/47162/c-how-do-i-add-a-tooltip-to-a-control
            //THMainResetTableButton
            _thToolTip = new ToolTip
            {
                // Set up the delays for the ToolTip.
                AutoPopDelay = 32000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                UseAnimation = true,
                UseFading = true,
                // Force the ToolTip text to be displayed whether or not the form is active.
                ShowAlways = true
            };
            _thToolTip.SetToolTip(MakeBuckupCheckBox,
                T._("Enables backup creation of selected game before mode switch\n" +
                " to be possible to restore Data and Mods dirs.\n" +
                "\n" +
                "Backup creating using ntfs hardlinks and not consumes any extra space.")
                );
        }
    }
}
