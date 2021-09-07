using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Base for sources
    /// </summary>
    abstract class SBase : IDisposable
    {
        protected UpdateInfo Info;

        protected SourceWebClient WC;

        Timer Timer;
        /// <summary>
        /// download file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="updateFilePath"></param>
        /// <returns></returns>
        protected async Task DownloadFileTaskAsync(Uri uri, string updateFilePath)
        {
            InitDownloadTimeTimer();
            try
            {
                await WC.DownloadFileTaskAsync(uri, updateFilePath).ConfigureAwait(true);
            }
            catch (WebException ex)
            {
                ManageLogs.Log("An error occured while file downloading. \r\nLink:" + Info.DownloadLink + "\r\nError:\r\n" + ex);
                Info.LastErrorText.AppendLine(" >" + ex.Message);
            }
        }

        /// <summary>
        /// made for causes when for some reason downloading is freezing like when reqrypt run
        /// </summary>
        private void InitDownloadTimeTimer()
        {
            Timer = new Timer
            {
                Interval = 10000 // 10 second timeout if download is freezed
            };
            Timer.Elapsed += CancelDownload;
            Timer.Start();
        }

        bool IsCancelDownload;
        private void CancelDownload(object sender, ElapsedEventArgs e)
        {
            IsCancelDownload = true;
            Info.LastErrorText.AppendLine("Download was freezed by some reason and was cancelled after 10 seconds elapsed.");
            WC.CancelAsync();
        }

        protected SBase(UpdateInfo info)
        {
            Info = info;
            WebClientInit();
        }

        private void WebClientInit()
        {
            WC = new SourceWebClient();

            WC.DownloadProgressChanged += DownloadProgressChanged;

            WC.DownloadFileCompleted += DownloadFileCompleted;
        }

        protected virtual void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RestartTimer();
        }

        private void RestartTimer()
        {
            // restart timer when progress is changing
            Timer.Stop();
            Timer.Start();
        }

        protected bool IsCompletedDownload;
        protected virtual void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(!IsCancelDownload)
            IsCompletedDownload = true;
            ReleaseTimer();
        }

        private void ReleaseTimer()
        {
            // stop timer and dispose
            Timer.Stop();
            Timer.Dispose();
        }

        protected SBase()
        {
        }

        /// <summary>
        /// url of selected web resource
        /// </summary>
        internal virtual string Url { get => ""; }

        /// <summary>
        /// source name for info purposes
        /// </summary>
        internal abstract string Title { get; }

        /// <summary>
        /// id for identidy updateinfo like 'updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::'
        /// </summary>
        internal abstract string InfoId { get; }

        /// <summary>
        /// function to get version of selected target
        /// </summary>
        /// <returns></returns>
        internal abstract string GetLastVersion();

        /// <summary>
        /// function to get download file from selected link
        /// </summary>
        /// <returns></returns>
        internal virtual byte[] DownloadFileFromTheLink(Uri link)
        {
            return null;
        }

        /// <summary>
        /// function of download update of selected file
        /// </summary>
        /// <returns></returns>
        internal abstract Task<bool> GetFile();

        public void Dispose()
        {
            if (WC != null)
            {
                WC.Dispose();
            }
        }
    }

    public class SourceWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = 5000; // 5 sec timeout
            return webRequest;
        }
    }
}
