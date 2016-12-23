using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetDisk
{
    public static class Authentication
    {
        public static bool IsLoggedIn(Credential credential)
        {
            var res = Operations.GetQuota(credential);
            if (res == null || res.errno != 0) return false;
            else return true;
        }
        public static LoginResult Login(string username, string password)
        {
            var result = new LoginResult();
            try
            {
                using (var wc = new CookieAwareWebClient())
                {
                    wc.DownloadData("http://pan.baidu.com/");
                    var str = wc.DownloadString("https://passport.baidu.com/v2/api/?getapi&tpl=netdisk&subpro=netdisk_web&apiver=v3");
                    var ltoken = Regex.Match(str, "\"token\"\\s*:\\s*\"(.+?)\"").Groups[1].Value;
                    var lstr = "token=" + ltoken + "&tpl=netdisk&username=" + Uri.EscapeDataString(username) + "&password=" + Uri.EscapeDataString(password);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    str = Encoding.UTF8.GetString(wc.UploadData("https://passport.baidu.com/v2/api/?login", Encoding.UTF8.GetBytes(lstr)));
                    var match = Regex.Match(str, "error=(\\d+)");
                    var errno = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                    if (errno != 0)
                    {
                        result.exception = new Exception("Login returned error = " + errno);
                    }
                    else
                    {
                        wc.DownloadData("https://passport.baidu.com/v3/login/api/auth/?return_type=3&tpl=netdisk&u=http%3A%2F%2Fpan.baidu.com%2Fdisk%2Fhome");
                        string baiduid = null, bduss = null, stoken = null;
                        foreach (Cookie cookie in wc.Cookies.GetAllCookies())
                        {
                            if (cookie.Name.ToLower() == "baiduid") baiduid = cookie.Value;
                            else if (cookie.Name.ToLower() == "bduss") bduss = cookie.Value;
                            else if (cookie.Name.ToLower() == "stoken" && cookie.Domain.ToLower().Contains("pan.")) stoken = cookie.Value;
                        }
                        if (baiduid != null && bduss != null && stoken != null)
                        {
                            result.credential = new Credential(baiduid, bduss, stoken);
                            result.success = true;
                        }
                        else result.exception = new Exception("Cannot find required cookies.");
                    }
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
            }
            return result;
        }
        public class LoginResult
        {
            public bool success;
            public Credential credential;
            public Exception exception;
        }
    }
}
