using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetDisk
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer Cookies { get; set; }
        public Uri Uri { get; set; }

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
