using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Manage;
using NLog;
using NLog.Fluent;

namespace AIHelper.Forms.Other
{
    public partial class DialogFormGeneral : Form
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger();
        public DialogFormGeneral()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            SetTranslations();
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

            _thToolTip.SetToolTip(btnStart,
                T._("Start process")
                );
            _thToolTip.SetToolTip(btnCancel,
                T._("Cancel process and close the form")
                );
            ////////////////////////////
        }
    }
}
