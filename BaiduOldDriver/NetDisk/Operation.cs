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
    public static class Operation
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
        public static FileListResult GetFileList(string path, Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res = wc.DownloadData("http://pan.baidu.com/api/list?page=1&num=10000000&dir=" + Uri.EscapeDataString(path));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(FileListResult));
                        var obj = ser.ReadObject(ms) as FileListResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new FileListResult() { exception = ex };
            }
        }
        public static ThumbnailResult GetThumbnail(string path, Credential credential, int width = 125, int height=90, int quality = 100)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res = wc.DownloadData("http://pcsdata.baidu.com/rest/2.0/pcs/thumbnail?app_id=250528&method=generate&path=" + Uri.EscapeDataString(path) + "&quality=" + quality + "&height=" + height + "&width=" + width);
                    return new ThumbnailResult() { success = true, image = res };
                }
            }
            catch (Exception ex)
            {
                return new ThumbnailResult() { exception = ex };
            }
        }
        public static GetDownloadResult GetDownload(string path, Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res = wc.DownloadData("http://d.pcs.baidu.com/rest/2.0/pcs/file?app_id=250528&method=locatedownload&ver=4.0&path=" + Uri.EscapeDataString(path));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(GetDownloadResult));
                        var obj = ser.ReadObject(ms) as GetDownloadResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new GetDownloadResult() { exception = ex };
            }
        }
    }
}
