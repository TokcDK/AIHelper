using System;
using System.Net;
using System.Net.Cache;

namespace AIHelper.Manage.Update.Sources
{
    //https://stackoverflow.com/a/11523836
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            CookieContainer = container;
            CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
        }

        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            if (webRequest is HttpWebRequest request)
            {
                request.CookieContainer = CookieContainer;
            }
            webRequest.Timeout = 5000; // 5 sec timeout
            return webRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            request.Timeout = 5000; // 5 sec timeout
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            if (r is HttpWebResponse response)
            {
                CookieCollection cookies = response.Cookies;
                CookieContainer.Add(cookies);
            }
        }
    }
}
