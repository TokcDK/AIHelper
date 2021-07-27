using System;
using System.Net;
using System.Threading.Tasks;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Base for sources
    /// </summary>
    abstract class SBase : IDisposable
    {
        protected UpdateInfo Info;

        protected readonly WebClient Wc;

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="updateFilePath"></param>
        /// <returns></returns>
        protected async Task DownloadFileTaskAsync(Uri uri, string updateFilePath)
        {
            try
            {
                await Wc.DownloadFileTaskAsync(uri, updateFilePath).ConfigureAwait(true);
            }
            catch (WebException ex)
            {
                ManageLogs.Log("An error occured while file downloading. \r\nLink:" + Info.DownloadLink + "\r\nError:\r\n" + ex);
                Info.LastErrorText.AppendLine(" >" + ex.Message);
            }
        }

        protected SBase(UpdateInfo info)
        {
            this.Info = info;
            Wc = new WebClient();
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
            if (Wc != null)
            {
                Wc.Dispose();
            }
        }
    }
}
