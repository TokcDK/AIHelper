using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    class ProgressForm : IDisposable
    {
        internal readonly Form PForm;
        readonly ProgressBar _progressBar;

        int Max;

        public ProgressForm()
        {
            Title = T._("Parse") + ":";

            PForm = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Width = 400,
                Height = 50,
                Text = Title,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                TopMost = true
            };
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = PForm.Height / 2,
                Width = PForm.Width - 2,
                Value = 0
            };

            PForm.Controls.Add(_progressBar);
            PForm.Show();
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
        /// Current progress value
        /// </summary>
        internal int Value { get => _progressBar.Value; }

        /// <summary>
        /// Info displaying in label
        /// </summary>
        /// <param name="infoText"></param>
        internal void SetInfo(string infoText)
        {
            PForm.Text = Title + ":" + infoText;
        }

        public void Dispose()
        {
            _progressBar.Dispose();
            PForm.Dispose();
        }
    }
}
