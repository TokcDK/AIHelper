using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Dialog with the options the data dir cleaner runs with.
    /// </summary>
    public partial class CleanOptionsDialogForm : Form
    {
        public CleanOptionsDialogForm()
        {
            InitializeComponent();
        }

        private void CleanOptionsDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the dialog is opened from a tool while the program runs, so it takes the
            // theme in use rather than the one which was current when it was created
            ThemesLoader.ApplyTheme(this);

            SetTooltips();
        }

        private void SetTranslations()
        {
            this.Text = T._("Cleaning options");
            cbxMoveIntoNewMod.Text = T._("Move to the new mod");
            cbxIgnoreSymlinks.Text = T._("Ignore symlinks");
            cbxIgnoreShortcuts.Text = T._("Ignore shortcuts");
            btnStart.Text = T._("OK");
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

            _toolTip.Set(cbxMoveIntoNewMod,
                T._("Instead of bakup folder removed files will be moved into new enabled mod in top of mods list"));
            _toolTip.Set(cbxIgnoreSymlinks, T._("Will ignore any symbolic links"));
            _toolTip.Set(cbxIgnoreShortcuts, T._("Will ignore any shortcuts"));
        }

        private void CleanOptionsDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeTooltips();
        }
    }
}
