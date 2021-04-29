using System;
using System.Net;
using System.Threading.Tasks;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Base for sources
    /// </summary>
    abstract class SBase
    {
        protected updateInfo info;

        protected readonly WebClient wc = new WebClient();

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
                await wc.DownloadFileTaskAsync(uri, updateFilePath).ConfigureAwait(true);
            }
            catch (WebException ex)
            {
                ManageLogs.Log("An error occured while file downloading. \r\nLink:" + info.DownloadLink + "\r\nError:\r\n" + ex);
                info.LastErrorText.AppendLine(ex.Message);
            }
        }

        protected SBase(updateInfo info)
        {
            this.info = info;
        }

        /// <summary>
        /// url of selected web resource
        /// </summary>
        internal virtual string url { get => ""; }

        /// <summary>
        /// source name for info purposes
        /// </summary>
        internal abstract string title { get; }

        /// <summary>
        /// id for identidy updateinfo like 'updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::'
        /// </summary>
        internal abstract string infoID { get; }

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
    }
}
