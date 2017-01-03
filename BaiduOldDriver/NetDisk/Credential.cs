using System.Net;

namespace NetDisk
{
    public class Credential
    {
        public string baiduid { get; }
        public string bduss { get; }
        public string stoken { get; }
        public long uid { get; }
        private string cookieString;
        public Credential(string baiduid, string bduss, string stoken, long uid)
        {
            this.baiduid = baiduid;
            this.bduss = bduss;
            this.stoken = stoken;
            this.uid = uid;
            cookieString = "BAIDUID=" + baiduid + "; BDUSS=" + bduss + "; STOKEN=" + stoken;
        }
        public static implicit operator string(Credential credential)
        {
            return credential.cookieString;
        }
        public static implicit operator CookieCollection(Credential credential)
        {
            var c = new CookieCollection();
            c.Add(new Cookie("BAIDUID", credential.baiduid, "/", ".baidu.com"));
            c.Add(new Cookie("BDUSS", credential.bduss, "/", ".baidu.com"));
            c.Add(new Cookie("STOKEN", credential.stoken, "/", ".pan.baidu.com"));
            return c;
        }
        public string Serialize()
        {
            return baiduid + "$" + bduss + "$" + stoken + "$" + uid;
        }
        public static Credential Deserialize(string str)
        {
            var tokens = str.Split('$');
            return new Credential(tokens[0], tokens[1], tokens[2], long.Parse(tokens[3]));
        }
    }
}
