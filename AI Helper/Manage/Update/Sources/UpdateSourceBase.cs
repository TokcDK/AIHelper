using NLog;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Base for update sources. A source knows how to find the latest version
    /// of a target and how to download the file for it.
    /// <para>
    /// One instance is used for a whole update run and is handed the current
    /// <see cref="UpdateInfo"/> by the owner (<see cref="Updater"/>) before
    /// every folder. Instances own a <see cref="WebClientEx"/> and a download
    /// watchdog timer, so they must be disposed by whoever created them.
    /// </para>
    /// </summary>
    abstract class UpdateSourceBase : IDisposable
    {
        protected static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// How long a download may go without reporting progress before it is
        /// considered frozen and cancelled.
        /// </summary>
        private const double DownloadWatchdogIntervalMilliseconds = 10000;

        /// <summary>
        /// Current update request state. Assigned by <see cref="Updater"/>
        /// before every folder is processed.
        /// </summary>
        internal UpdateInfo Info;

        protected WebClientEx WebClient;

        /// <summary>
        /// When true the source is skipped for the rest of the session, for
        /// example after hitting a GitHub rate limit.
        /// </summary>
        internal bool IsNotWorkingNow;

        private System.Timers.Timer _downloadWatchdog;
        private bool _isCancelDownload;
        private bool _isCompletedDownload;
        private bool _disposed;

        /// <summary>True when the last download finished without being cancelled.</summary>
        protected bool IsCompletedDownload
        {
            get { return _isCompletedDownload; }
        }

        /// <summary>True when the watchdog cancelled the last download.</summary>
        protected bool IsCancelDownload
        {
            get { return _isCancelDownload; }
        }

        protected UpdateSourceBase(UpdateInfo info)
        {
            Info = info;
            WebClientInit();
        }

        /// <summary>
        /// Downloads <paramref name="uri"/> to <paramref name="updateFilePath"/>.
        /// Never throws: a failure is logged, recorded in
        /// <see cref="UpdateInfo"/> and reported through the return value.
        /// </summary>
        /// <param name="uri">Remote file.</param>
        /// <param name="updateFilePath">Local destination path.</param>
        /// <returns>True when the file was downloaded and is not empty.</returns>
        protected async Task<bool> DownloadFileTaskAsync(Uri uri, string updateFilePath)
        {
            StartDownloadWatchdog();

            // a big archive needs a much larger budget than a release page
            var previousTimeout = WebClient.RequestTimeoutMilliseconds;
            WebClient.RequestTimeoutMilliseconds = WebClientEx.DownloadRequestTimeoutMilliseconds;

            try
            {
                await WebClient.DownloadFileTaskAsync(uri, updateFilePath).ConfigureAwait(true);
            }
            catch (WebException ex)
            {
                _log.Warn(ex, "An error occured while file downloading.\r\nLink:" + Info.DownloadLink);
                Info.AppendError(ex.Message);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "An unexpected error occured while file downloading.\r\nLink:" + Info.DownloadLink);
                Info.AppendError(ex.Message);
            }
            finally
            {
                WebClient.RequestTimeoutMilliseconds = previousTimeout;
                StopDownloadWatchdog();
            }

            return IsCompletedDownload && UpdateIo.GetFileLengthOrZero(updateFilePath) != 0;
        }

        private void WebClientInit()
        {
            if (WebClient == null)
            {
                WebClient = new WebClientEx(new CookieContainer())
                {
                    Encoding = System.Text.Encoding.UTF8
                };

                WebClient.Headers.Add(HttpRequestHeader.UserAgent, UserAgents.ChromeIronWin7);
            }

            WebClient.DownloadProgressChanged += DownloadProgressChanged;
            WebClient.DownloadFileCompleted += DownloadFileCompleted;
        }

        protected virtual void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RestartDownloadWatchdog();
        }

        /// <summary>
        /// Starts (or restarts) the freeze watchdog for a new download and
        /// clears the flags of the previous one.
        /// </summary>
        private void StartDownloadWatchdog()
        {
            StopDownloadWatchdog();

            _isCancelDownload = false;
            _isCompletedDownload = false;

            _downloadWatchdog = new System.Timers.Timer(DownloadWatchdogIntervalMilliseconds)
            {
                // fire once instead of every interval
                AutoReset = false
            };
            _downloadWatchdog.Elapsed += DownloadWatchdogElapsed;
            _downloadWatchdog.Start();
        }

        private void RestartDownloadWatchdog()
        {
            var watchdog = _downloadWatchdog;
            if (watchdog == null) return;

            try
            {
                watchdog.Stop();
                watchdog.Start();
            }
            catch (ObjectDisposedException)
            {
                // the download finished while this progress event was in flight
            }
        }

        private void StopDownloadWatchdog()
        {
            var watchdog = _downloadWatchdog;
            if (watchdog == null) return;

            _downloadWatchdog = null;

            try
            {
                watchdog.Stop();
                watchdog.Elapsed -= DownloadWatchdogElapsed;
                watchdog.Dispose();
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to release the download watchdog. error:" + Environment.NewLine + ex);
            }
        }

        private void DownloadWatchdogElapsed(object sender, ElapsedEventArgs e)
        {
            _isCancelDownload = true;
            Info.AppendError("Download was freezed by some reason and was cancelled after 10 seconds elapsed.");

            try
            {
                WebClient.CancelAsync();
            }
            catch (Exception ex)
            {
                _log.Debug("Failed to cancel a frozen download. error:" + Environment.NewLine + ex);
            }
        }

        protected virtual void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!_isCancelDownload) _isCompletedDownload = true;

            StopDownloadWatchdog();
        }

        /// <summary>
        /// Url of the selected web resource, used to build download links.
        /// </summary>
        internal virtual string Url
        {
            get { return ""; }
        }

        /// <summary>
        /// Source name for info purposes.
        /// </summary>
        internal abstract string Title { get; }

        /// <summary>
        /// Id used to recognise an update record, for example
        /// 'updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::'.
        /// </summary>
        internal abstract string InfoId { get; }

        /// <summary>
        /// Gets the latest version of the currently selected target.
        /// </summary>
        /// <returns>Version string, or empty when it could not be resolved.</returns>
        internal abstract string GetLastVersion();

        /// <summary>
        /// Downloads the file of the currently selected target.
        /// </summary>
        /// <returns>True when a usable file is available on disk.</returns>
        internal abstract Task<bool> GetFile();

        /// <summary>
        /// Reads a small resource into memory. Returns an empty array on
        /// failure instead of throwing.
        /// </summary>
        /// <param name="link">Resource to read.</param>
        /// <returns>Payload bytes, or an empty array.</returns>
        internal virtual byte[] DownloadFileFromTheLink(Uri link)
        {
            return Array.Empty<byte>();
        }

        /// <summary>
        /// True when the source must be skipped for the rest of the session.
        /// </summary>
        /// <param name="info">Current update state.</param>
        internal virtual bool CheckIfStopWork(UpdateInfo info)
        {
            return false;
        }

        /// <summary>
        /// Waits a little before the next request to the same source, so the
        /// updater does not look like a scraper. Awaitable on purpose: the
        /// update pass runs on the UI thread and must not be blocked.
        /// </summary>
        internal virtual Task PauseAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;

            if (!disposing) return;

            StopDownloadWatchdog();

            if (WebClient != null)
            {
                WebClient.Dispose();
                WebClient = null;
            }
        }
    }
}
