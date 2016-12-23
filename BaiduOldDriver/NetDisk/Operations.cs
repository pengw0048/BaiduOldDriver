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
            if (credential == null) return null;
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
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new QuotaResult() { ex = ex };
            }
        }
    }
}
