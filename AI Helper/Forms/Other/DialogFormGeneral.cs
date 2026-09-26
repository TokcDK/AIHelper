using AIHelper.Manage;
using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Dialog which shows a set of options and an accept button, used by the tools
    /// which need a confirmation before they do something.
    /// </summary>
    /// <remarks>
    /// The options are added by the caller into <see cref="flpOptions"/>, so the
    /// dialog itself does not have to know what it is asking about.
    /// </remarks>
    public partial class DialogFormGeneral : Form
    {
        public DialogFormGeneral()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the dialog is opened from a tool while the program runs, so it takes the
            // theme in use rather than the one which was current when it was created
            ThemesLoader.ApplyTheme(this);

            SetTooltips();

            this.Location = ManageSettings.MainForm.Location;
            this.Size = ManageSettings.MainForm.Size;
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            DisposeTooltips();
        }

        private void SetTranslations()
        {
            this.Text = T._("Options");
            btnStart.Text = T._("Start");
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

            _toolTip.Set(btnStart, T._("Start process"));
            _toolTip.Set(btnCancel, T._("Cancel process and close the form"));
        }
    }
}
