using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace AIHelper.Forms.Other
{
    public partial class CleanOptionsDialogForm : Form
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public CleanOptionsDialogForm()
        {
            InitializeComponent();
        }

        private void CleanOptionsDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();
            SetTooltips();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SetTranslations()
        {
            this.Text = T._("Cleaning options");
            cbxMoveToNewMod.Text = T._("Move to the new mod");
            cbxIgnoreSymlinks.Text = T._("Ignore symlinks");
            btnStart.Text = T._("Start");
            btnCancel.Text = T._("Cancel");
        }

        private ToolTip _thToolTip;

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
                _log.Error("An error occured while SetTooltips. error:\r\n" + ex);
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

            _thToolTip.SetToolTip(cbxMoveToNewMod,
                T._("Instead of bakup folder removed files will be moved into new enabled mod in top of mods list")
                );
            _thToolTip.SetToolTip(cbxIgnoreSymlinks,
                T._("Will ignore any symbolic links")
                );
            ////////////////////////////
        }

        private void CleanOptionsDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeTooltips();
        }
    }
}
