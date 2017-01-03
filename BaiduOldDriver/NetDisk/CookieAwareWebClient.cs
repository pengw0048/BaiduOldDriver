using System;
using System.Net;

namespace NetDisk
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer Cookies { get; set; }
        public Uri Uri { get; set; }
        Uri _responseUri;
        public Uri ResponseUri
        {
            get { return _responseUri; }
        }
        public CookieAwareWebClient()
            : this(new CookieContainer())
        {
        }

        public CookieAwareWebClient(CookieContainer cookies)
        {
            this.Cookies = cookies;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                ClearCookiesVersion();
                (request as HttpWebRequest).CookieContainer = this.Cookies;
            }
            HttpWebRequest httpRequest = (HttpWebRequest)request;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return httpRequest;
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            _responseUri = response.ResponseUri;
            return response;
        }
        private void ClearCookiesVersion()
        {
            var cc = new CookieContainer();
            foreach (Cookie cookie in Cookies.GetAllCookies())
            {
                cookie.Version = 0;
                cc.Add(cookie);
            }
            Cookies = cc;
        }
    }
}
