using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetDisk
{
    public class Credential
    {
        public string baiduid { get; }
        public string bduss { get; }
        public string stoken { get; }
        private string cookieString;
        public Credential(string baiduid, string bduss, string stoken)
        {
            this.baiduid = baiduid;
            this.bduss = bduss;
            this.stoken = stoken;
            cookieString = "BAIDUID=" + baiduid + "; BDUSS=" + bduss + "; STOKEN=" + stoken;
        }
        public static implicit operator string(Credential credential)
        {
            return credential.cookieString;
        }
    }
}
