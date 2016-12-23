using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetDisk
{
    public static class Operations
    {
        public static QuotaResult GetQuota(Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res = wc.DownloadData("http://pan.baidu.com/api/quota?checkexpire=1&checkfree=1");
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(QuotaResult));
                        var obj = ser.ReadObject(ms) as QuotaResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new QuotaResult() { exception = ex };
            }
        }
        public static UserInfoResult GetUserInfo(Credential credential)
        {
            try
            {
                if (credential.uid <= 0) throw new Exception("Invalid uid.");
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res = wc.DownloadData("http://pan.baidu.com/api/user/getinfo?user_list=[" + credential.uid + "]");
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(UserInfoResult));
                        var obj = ser.ReadObject(ms) as UserInfoResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new UserInfoResult() { exception = ex };
            }
        }
    }
}
