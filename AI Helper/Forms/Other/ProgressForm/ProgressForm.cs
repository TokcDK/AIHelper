using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    class ProgressForm:IDisposable
    {
        Form _progressForm;
        ProgressBar _progressBar;
        Label _progressInfoLabel;

        int Max;

        public ProgressForm()
        {
            Title = T._("Parse") + ":";

            _progressForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(400, 100),
                Text = Title,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                TopMost = true
            };
            _progressInfoLabel = new Label
            {
                Dock = DockStyle.Top,
                Text = ""
            };
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Value = 0
            };

            _progressForm.Controls.Add(_progressBar);
            _progressForm.Show();
        }

        bool IsSet;
        /// <summary>
        /// Maxumum value of progressbar.
        /// Can be set only one time.
        /// </summary>
        /// <param name="maxProgressBarValue"></param>
        internal void SetMax(int maxProgressBarValue)
        {
            if (IsSet)
            {
                return;
            }

            IsSet = true;

            Max = maxProgressBarValue;
            _progressBar.Maximum = Max;
        }

        internal string Title;

        /// <summary>
        /// Progressbar value
        /// </summary>
        /// <param name="progressBarValue"></param>
        internal void SetProgress(int progressBarValue)
        {
            if (progressBarValue <= Max)
            {
                _progressBar.Value = progressBarValue;
            }
        }

        /// <summary>
        /// Info displaying in label
        /// </summary>
        /// <param name="infoText"></param>
        internal void SetInfo(string infoText)
        {
            _progressInfoLabel.Text = infoText;
        }

        public void Dispose()
        {
            _progressForm.Dispose();
            _progressBar.Dispose();
            _progressInfoLabel.Dispose();
        }
    }
}
