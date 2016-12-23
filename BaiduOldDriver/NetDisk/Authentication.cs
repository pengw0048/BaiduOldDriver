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
        public static LoginResult Login(string username, string password, LoginCheckResult checkResult)
        {
            var result = new LoginResult();
            try
            {
                using (var wc = new CookieAwareWebClient())
                {
                    wc.Cookies.Add(checkResult.baiduid);
                    var ltoken = checkResult.ltoken;
                    var lstr = "token=" + ltoken + "&tpl=netdisk&username=" + Uri.EscapeDataString(username) + "&password=" + Uri.EscapeDataString(password);
                    if (checkResult.needVCode) lstr += "&codestring=" + checkResult.codeString + "&verifycode=" + Uri.EscapeDataString(checkResult.verifyCode);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    var str = Encoding.UTF8.GetString(wc.UploadData("https://passport.baidu.com/v2/api/?login", Encoding.UTF8.GetBytes(lstr)));
                    var match = Regex.Match(str, "error=(\\d+)");
                    var errno = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                    if (errno != 0)
                    {
                        result.exception = new Exception("Login returned error = " + errno);
                    }
                    else
                    {
                        str = wc.DownloadString("https://passport.baidu.com/v3/login/api/auth/?return_type=3&tpl=netdisk&u=http%3A%2F%2Fpan.baidu.com%2Fdisk%2Fhome");
                        long uid = 0;
                        match = Regex.Match(str, "\"uk\"\\s*:\\s*(\\d+)");
                        if (match.Success) long.TryParse(match.Groups[1].Value, out uid);
                        string baiduid = null, bduss = null, stoken = null;
                        foreach (Cookie cookie in wc.Cookies.GetAllCookies())
                        {
                            if (cookie.Name.ToLower() == "baiduid") baiduid = cookie.Value;
                            else if (cookie.Name.ToLower() == "bduss") bduss = cookie.Value;
                            else if (cookie.Name.ToLower() == "stoken" && cookie.Domain.ToLower().Contains("pan.")) stoken = cookie.Value;
                        }
                        if (baiduid != null && bduss != null && stoken != null)
                        {
                            result.credential = new Credential(baiduid, bduss, stoken, uid);
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
        public static LoginCheckResult LoginCheck(string username)
        {
            var result = new LoginCheckResult();
            try
            {
                using (var wc = new CookieAwareWebClient())
                {
                    wc.DownloadData("http://pan.baidu.com/");
                    Cookie baiduid = null;
                    foreach (Cookie cookie in wc.Cookies.GetAllCookies())
                    {
                        if (cookie.Name.ToLower() == "baiduid") baiduid = cookie;
                    }
                    if (baiduid == null) throw new Exception("Cannot obtain BAIDUID.");
                    result.baiduid = baiduid;
                    var str = wc.DownloadString("https://passport.baidu.com/v2/api/?getapi&tpl=netdisk&subpro=netdisk_web&apiver=v3");
                    var ltoken = Regex.Match(str, "\"token\"\\s*:\\s*\"(.+?)\"").Groups[1].Value;
                    result.ltoken = ltoken;
                    str = wc.DownloadString("https://passport.baidu.com/v2/api/?logincheck&token=" + ltoken + "&tpl=netdisk&subpro=netdisk_web&apiver=v3&username=" + Uri.EscapeDataString(username));
                    var codeString = Regex.Match(str, "\"codeString\"\\s*:\\s*\"(.*?)\"").Groups[1].Value;
                    if (codeString == "")
                    {
                        result.success = true;
                    }
                    else
                    {
                        result.image = wc.DownloadData("https://passport.baidu.com/cgi-bin/genimage?" + codeString);
                        result.success = true;
                        result.needVCode = true;
                        result.codeString = codeString;
                    }
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
            }
            return result;
        }
    }
}
