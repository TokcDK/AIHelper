using AIHelper.Install;
using AIHelper.Manage;
using AIHelper.Manage.Logging;
using AIHelper.Manage.ui.themes;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Window which runs one install pass and shows how far it has come.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The window opens when the install button is pressed but does nothing until its
    /// own install button is pressed, so the folder can be checked first.
    /// </para>
    /// <para>
    /// Two progress bars are shown: one for the whole run (which installer is
    /// running) and one for the entries of the running installer. Both are fed by
    /// <see cref="InstallProgress"/>, which reports from the thread the installers
    /// run on, so every update is marshalled to the UI thread here.
    /// </para>
    /// <para>
    /// The log box is fed by <see cref="InstallLogTarget"/>, which is an NLog target
    /// added next to the file target, so the window shows exactly what is written to
    /// the log file. Entries are collected in a queue and appended on a timer instead
    /// of once per entry, because a chatty install would otherwise flood the message
    /// pump.
    /// </para>
    /// </remarks>
    public partial class InstallProgressForm : Form
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Number of log lines kept in the box. Older lines are dropped so a long
        /// install can not make the window slower and slower.
        /// </summary>
        private const int MaxLogLines = 2000;

        private readonly Action _installAction;

        private readonly ConcurrentQueue<string> _pendingLogLines = new ConcurrentQueue<string>();
        private readonly List<string> _logLines = new List<string>();

        private bool _isRunning;
        private bool _isDetached;

        public InstallProgressForm(Action installAction)
        {
            _installAction = installAction;

            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the window is opened from the tools tab, so it gets the same theme as
            // the rest of the application
            if (ManageSettings.CurrentTheme != null)
            {
                ThemesLoader.SetTheme(ManageSettings.CurrentTheme, this);
            }

            ResetToReadyState();

            InstallLogTarget.EnsureRegistered();

            InstallLogTarget.EntryWritten += OnLogEntryWritten;
            InstallProgress.RunStarted += OnRunStarted;
            InstallProgress.RunFinished += OnRunFinished;
            InstallProgress.StepStarted += OnStepStarted;
            InstallProgress.StepProgressChanged += OnStepProgressChanged;

            logFlushTimer.Start();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // a running install can not be cancelled, so the window must stay until it
            // is done, otherwise the user would lose sight of what is happening
            if (_isRunning)
            {
                e.Cancel = true;
                return;
            }

            Detach();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            StartInstall();
        }

        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            OpenInstallFolder();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Runs the install on a background thread and keeps the window responsive.
        /// </summary>
        private async void StartInstall()
        {
            if (_isRunning || _installAction == null) return;

            _isRunning = true;
            SetRunningState();

            try
            {
                await Task.Run(_installAction).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to install mods. Error:\r\n" + ex);
            }
            finally
            {
                _isRunning = false;

                // the run normally reports its own end through InstallProgress, this
                // only covers a run which failed before it could report
                SetFinishedState();
            }
        }

        private void OpenInstallFolder()
        {
            try
            {
                Directory.CreateDirectory(ManageSettings.Install2MoDirPath);

                Process.Start("explorer.exe", ManageSettings.Install2MoDirPath);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to open the install folder. Error:\r\n" + ex);
            }
        }

        private void LogFlushTimer_Tick(object sender, EventArgs e)
        {
            if (_pendingLogLines.IsEmpty) return;

            var newLines = new List<string>();

            while (_pendingLogLines.TryDequeue(out var line))
            {
                newLines.Add(line);
            }

            _logLines.AddRange(newLines);

            if (_logLines.Count > MaxLogLines)
            {
                // the box has to be rewritten from scratch when the oldest lines are
                // dropped, which is why a whole batch is kept instead of one line
                _logLines.RemoveRange(0, _logLines.Count - MaxLogLines);

                txtLog.Text = string.Join(Environment.NewLine, _logLines);
            }
            else
            {
                txtLog.AppendText(string.Join(Environment.NewLine, newLines) + Environment.NewLine);
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void OnLogEntryWritten(string message)
        {
            // called on the thread which wrote the log entry, so only the queue is
            // touched here
            _pendingLogLines.Enqueue(message);
        }

        private void OnRunStarted(int stepCount)
        {
            RunOnUiThread(() =>
            {
                progressOverall.Maximum = Math.Max(1, stepCount);
                progressOverall.Value = 0;

                progressStep.Maximum = 1;
                progressStep.Value = 0;

                lblStep.Text = string.Empty;
                lblStatus.Text = T._("Installing") + "...";
            });
        }

        private void OnStepStarted(int stepNumber, int stepCount, string stepName)
        {
            RunOnUiThread(() =>
            {
                progressOverall.Maximum = Math.Max(1, stepCount);
                progressOverall.Value = Clamp(stepNumber - 1, progressOverall.Maximum);

                lblStatus.Text = T._("Step") + " " + stepNumber + "/" + stepCount + ": " + stepName;

                progressStep.Maximum = 1;
                progressStep.Value = 0;
                lblStep.Text = string.Empty;
            });
        }

        private void OnStepProgressChanged(int processedCount, int entryCount, string entryName)
        {
            RunOnUiThread(() =>
            {
                progressStep.Maximum = Math.Max(1, entryCount);
                progressStep.Value = Clamp(processedCount, progressStep.Maximum);

                lblStep.Text = entryName;
            });
        }

        private void OnRunFinished()
        {
            RunOnUiThread(SetFinishedState);
        }

        /// <summary>
        /// Brings the window back into the state it has before a run.
        /// </summary>
        private void ResetToReadyState()
        {
            progressOverall.Maximum = 1;
            progressOverall.Value = 0;
            progressStep.Maximum = 1;
            progressStep.Value = 0;

            lblStatus.Text = T._("Ready to install");
            lblStep.Text = string.Empty;
            lblFolder.Text = T._("Install folder") + ": " + ManageSettings.Install2MoDirPath;

            btnStart.Enabled = true;
            btnOpenFolder.Enabled = true;
            btnClose.Enabled = true;
        }

        private void SetRunningState()
        {
            btnStart.Enabled = false;
            btnOpenFolder.Enabled = true;

            // the install can not be cancelled, so the window must not be closed while
            // it runs
            btnClose.Enabled = false;

            lblStatus.Text = T._("Installing") + "...";
        }

        private void SetFinishedState()
        {
            progressOverall.Value = progressOverall.Maximum;

            progressStep.Maximum = 1;
            progressStep.Value = 1;
            lblStep.Text = string.Empty;

            lblStatus.Text = T._("Finished. See the log for details.");

            btnStart.Enabled = false;
            btnOpenFolder.Enabled = true;
            btnClose.Enabled = true;
        }

        private void Detach()
        {
            _isDetached = true;

            logFlushTimer.Stop();

            InstallLogTarget.EntryWritten -= OnLogEntryWritten;
            InstallProgress.RunStarted -= OnRunStarted;
            InstallProgress.RunFinished -= OnRunFinished;
            InstallProgress.StepStarted -= OnStepStarted;
            InstallProgress.StepProgressChanged -= OnStepProgressChanged;
        }

        private void SetTranslations()
        {
            Text = T._("Install mods");

            btnStart.Text = T._("Install");
            btnOpenFolder.Text = T._("Open install folder");
            btnClose.Text = T._("Close");
        }

        /// <summary>
        /// Runs <paramref name="action"/> on the UI thread, because the installers
        /// report from a background thread.
        /// </summary>
        private void RunOnUiThread(Action action)
        {
            if (_isDetached || IsDisposed || Disposing) return;

            if (!InvokeRequired)
            {
                action();
                return;
            }

            try
            {
                BeginInvoke((Action)(() =>
                {
                    // the window can be closed between the check above and this call
                    if (_isDetached || IsDisposed || Disposing) return;

                    action();
                }));
            }
            catch (InvalidOperationException)
            {
                // the handle is already gone, which means the window was closed and
                // the update is not needed any more
            }
        }

        private static int Clamp(int value, int maximum)
        {
            if (value < 0) return 0;
            return value > maximum ? maximum : value;
        }
    }
}
