using System.Threading.Tasks;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// Base for sources
    /// </summary>
    abstract class SBase
    {
        protected updateInfo info;

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
        /// function of download update of selected file
        /// </summary>
        /// <returns></returns>
        internal abstract Task<bool> GetFile();
    }
}
