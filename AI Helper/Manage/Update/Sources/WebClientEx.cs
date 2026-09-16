using System;
using System.Net;
using System.Net.Cache;

namespace AIHelper.Manage.Update.Sources
{
    /// <summary>
    /// <see cref="WebClient"/> with a shared cookie container, an explicit
    /// timeout and HTTP caching disabled.
    /// <para>
    /// Caching is disabled on purpose: the updater asks GitHub for the
    /// "latest" release page, and answering that from the WinINet cache would
    /// silently hide new versions. The timeout is configurable through
    /// <see cref="RequestTimeoutMilliseconds"/> because a release page and a
    /// multi-hundred-megabyte mod archive need very different budgets.
    /// </para>
    /// <para>
    /// Instances are not thread-safe, exactly like the base class. The cookie
    /// container is shared with the caller and is never null.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Cookie handling follows https://stackoverflow.com/a/11523836
    /// </remarks>
    internal sealed class WebClientEx : WebClient
    {
        /// <summary>Timeout used for page and API reads.</summary>
        internal const int DefaultRequestTimeoutMilliseconds = 5000;

        /// <summary>Timeout used while a mod archive is being downloaded.</summary>
        internal const int DownloadRequestTimeoutMilliseconds = 5 * 60 * 1000;

        internal WebClientEx(CookieContainer container)
        {
            CookieContainer = container ?? new CookieContainer();
            CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
        }

        /// <summary>
        /// Cookie container applied to every <see cref="HttpWebRequest"/>.
        /// Never null.
        /// </summary>
        internal CookieContainer CookieContainer { get; set; } = new CookieContainer();

        /// <summary>
        /// Response timeout in milliseconds. Defaults to
        /// <see cref="DefaultRequestTimeoutMilliseconds"/>; raise it to
        /// <see cref="DownloadRequestTimeoutMilliseconds"/> around a file
        /// download and restore it afterwards.
        /// </summary>
        internal int RequestTimeoutMilliseconds { get; set; } = DefaultRequestTimeoutMilliseconds;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);

            // base returns null for schemes it does not handle
            if (webRequest == null) return null;

            if (webRequest is HttpWebRequest httpRequest)
            {
                httpRequest.CookieContainer = CookieContainer;
            }

            webRequest.Timeout = RequestTimeoutMilliseconds;
            return webRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            var response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse response)
        {
            if (response is HttpWebResponse httpResponse)
            {
                CookieContainer.Add(httpResponse.Cookies);
            }
        }
    }
}
