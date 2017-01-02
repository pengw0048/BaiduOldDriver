using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "netdisk;5.4.5.4;PC;PC-Windows;10.0.14393;WindowsBaiduYunGuanJia");
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
        public static FileOperationResult CreateFolder(string path, Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    var str = "isdir=1&path=" + Uri.EscapeDataString(path);
                    var res = wc.UploadData("http://pan.baidu.com/api/create?a=commit", Encoding.UTF8.GetBytes(str));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(FileOperationResult));
                        var obj = ser.ReadObject(ms) as FileOperationResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new FileOperationResult() { exception = ex };
            }
        }
        public static FileOperationResult Copy(string path, string dest, string newname, Credential credential)
        {
            var str = "filelist=[{\"path\":\"" + Uri.EscapeDataString(path) + "\",\"dest\":\"" + Uri.EscapeDataString(dest) + "\",\"newname\":\"" + Uri.EscapeDataString(newname) + "\"}]";
            return FileOp("http://pan.baidu.com/api/filemanager?opera=copy&clienttype=8", str, credential);
        }
        public static FileOperationResult Delete(string path, Credential credential)
        {
            var str = "filelist=[\"" + Uri.EscapeDataString(path) + "\"]";
            return FileOp("http://pan.baidu.com/api/filemanager?opera=delete&clienttype=8", str, credential);
        }
        public static FileOperationResult Move(string path, string dest, string newname, Credential credential)
        {
            var str = "filelist=[{\"path\":\"" + Uri.EscapeDataString(path) + "\",\"dest\":\"" + Uri.EscapeDataString(dest) + "\",\"newname\":\"" + Uri.EscapeDataString(newname) + "\"}]";
            return FileOp("http://pan.baidu.com/api/filemanager?opera=move&clienttype=8", str, credential);
        }
        public static FileOperationResult Rename(string path, string newname, Credential credential)
        {
            var str = "filelist=[{\"path\":\"" + Uri.EscapeDataString(path) + "\",\"newname\":\"" + Uri.EscapeDataString(newname) + "\"}]";
            return FileOp("http://pan.baidu.com/api/filemanager?opera=rename&clienttype=8", str, credential);
        }
        private static FileOperationResult FileOp(string url, string str, Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    var res = wc.UploadData(url, Encoding.UTF8.GetBytes(str));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(FileOpResult));
                        var obj = ser.ReadObject(ms) as FileOpResult;
                        if (obj.info.Length == 0 && obj.errno != 0)
                            return new FileOperationResult() { success = true, errno = obj.errno };
                        else if (obj.info.Length == 0 || obj.errno != 0 && obj.info[0].errno == 0)
                            throw new Exception("Response data malformat.");
                        else
                            return new FileOperationResult() { success = true, errno = obj.info[0].errno, path = obj.info[0].path };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FileOperationResult() { exception = ex };
            }
        }
        public static OfflineListResult GetOfflineList(Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    var res1 = wc.DownloadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528&method=list_task&need_task_info=1&status=255");
                    using (var ms1 = new MemoryStream(res1))
                    {
                        var ser1 = new DataContractJsonSerializer(typeof(ListTaskResult));
                        var ltr = ser1.ReadObject(ms1) as ListTaskResult;
                        if (ltr.task_info.Length == 0) return new OfflineListResult() { success = true, tasks = new OfflineListResult.Entry[0] };
                        var str = "method=query_task&op_type=1&task_ids=" + Uri.EscapeDataString(string.Join(",", ltr.task_info.Select(e => e.task_id.ToString())));
                        wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                        var res2 = wc.UploadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528", Encoding.UTF8.GetBytes(str));
                        var qtr = JsonConvert.DeserializeObject<QueryTaskResult>(Encoding.UTF8.GetString(res2));
                        return new OfflineListResult()
                        {
                            tasks = ltr.task_info.Select(e =>
                            {
                                var ai = qtr.task_info[e.task_id.ToString()];
                                return new OfflineListResult.Entry()
                                {
                                    create_time = e.create_time,
                                    od_type = e.od_type,
                                    save_path = e.save_path,
                                    source_url = e.source_url,
                                    task_id = e.task_id,
                                    task_name = e.task_name,
                                    file_size = ai.file_size,
                                    finished_size = ai.finished_size,
                                    status = ai.status
                                };
                            }).ToArray(),
                            success = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new OfflineListResult() { exception = ex };
            }
        }
        public static Result CancelOfflineTask(long taskid, Credential credential)
        {
            return OfflineTaskOp(taskid, "cancel_task", credential);
        }
        public static Result DeleteOfflineTask(long taskid, Credential credential)
        {
            return OfflineTaskOp(taskid, "delete_task", credential);
        }
        private static Result OfflineTaskOp(long taskid, string oper, Credential credential)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    wc.DownloadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528&method=" + oper + "&task_id=" + taskid);
                    return new Result() { success = true };
                }
            }
            catch (Exception ex)
            {
                return new Result() { exception = ex };
            }
        }
        public static QueryLinkResult QueryLinkFiles(string link, Credential credential)
        {
            try
            {
                using(var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    if (link.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
                    {
                        var res = wc.DownloadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528&method=query_magnetinfo&source_url=" + Uri.EscapeDataString(link));
                        using (var ms = new MemoryStream(res))
                        {
                            var ser = new DataContractJsonSerializer(typeof(QueryMagnetResult));
                            var obj = ser.ReadObject(ms) as QueryMagnetResult;
                            return new QueryLinkResult() { success = true, files = obj.magnet_info.file_info };
                        }
                    }
                    else if (link.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
                    {
                        var res = wc.DownloadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528&method=query_sinfo&type=2&source_path=" + Uri.EscapeDataString(link));
                        using (var ms = new MemoryStream(res))
                        {
                            var ser = new DataContractJsonSerializer(typeof(QueryTorrentResult));
                            var obj = ser.ReadObject(ms) as QueryTorrentResult;
                            return new QueryLinkResult() { success = true, files = obj.torrent_info.file_info };
                        }
                    }
                    else throw new Exception("Not a magnet link or a torrent file.");
                }
            }
            catch(Exception ex)
            {
                return new QueryLinkResult() { exception = ex };
            }
        }
        public static AddOfflineTaskResult AddOfflineTask(string link, string savepath, Credential credential, int[] selected = null, string sha1 = "")
        {
            try
            {
                var str = "method=add_task&save_path=" + Uri.EscapeDataString(savepath) + "&";
                if (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    str += "type=0&source_url=" + Uri.EscapeDataString(link);
                else if (link.StartsWith("ed2k://", StringComparison.OrdinalIgnoreCase))
                    str += "type=3&source_url=" + Uri.EscapeDataString(link);
                else if (link.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
                    str += "type=4&task_from=5&source_url=" + Uri.EscapeDataString(link) + "&selected_idx=" + string.Join(",", selected.Select(i => i.ToString()));
                else if (link.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))
                    str += "type=2&task_from=5&file_sha1=" + sha1 + "&source_path=" + Uri.EscapeDataString(link) + "&selected_idx=" + string.Join(",", selected.Select(i => i.ToString()));
                else throw new Exception("Link invalid.");
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    var res = wc.UploadData("http://pan.baidu.com/rest/2.0/services/cloud_dl?app_id=250528", Encoding.UTF8.GetBytes(str));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(AddOfflineTaskResult));
                        var obj = ser.ReadObject(ms) as AddOfflineTaskResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new AddOfflineTaskResult() { exception = ex };
            }
        }
        public static ShareResult Share(string[] pathlist, Credential credential, string pwd = null)
        {
            try
            {
                if (pwd != null && pwd.Length != 4) throw new Exception("Length of pwd must be 4.");
                var str = "path_list=[" + string.Join(",", pathlist.Select(p => '"' + p + '"')) + "]&channel_list=[]&shorturl=1&";
                if (pwd == null) str += "public=1&schannel=0";
                else str += "public=0&schannel=4&pwd=" + pwd;
                var rand = new Random();
                var logid = new string(Enumerable.Range(0, 100).Select(i => (char)('a' + rand.Next(26))).ToArray());
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.Cookie, credential);
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    var res = wc.UploadData("http://pan.baidu.com/share/pset?clienttype=8&channel=00000000000000000000000000000000&version=5.4.5.4&devuid=123456&logid=" + logid, Encoding.UTF8.GetBytes(str));
                    using (var ms = new MemoryStream(res))
                    {
                        var ser = new DataContractJsonSerializer(typeof(ShareResult));
                        var obj = ser.ReadObject(ms) as ShareResult;
                        obj.success = true;
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                return new ShareResult() { exception = ex };
            }
        }
        [DataContract]
        private class FileOpResult
        {
            [DataMember]
            public int errno;
            [DataMember]
            public Entry[] info;
            [DataContract]
            public class Entry
            {
                [DataMember]
                public int errno;
                [DataMember]
                public string path;
            }
        }
        [DataContract]
        private class ListTaskResult
        {
            [DataMember]
            public Entry[] task_info;
            [DataContract]
            public class Entry
            {
                [DataMember]
                public long create_time;
                [DataMember]
                public int od_type;
                [DataMember]
                public string save_path;
                [DataMember]
                public string source_url;
                [DataMember]
                public long task_id;
                [DataMember]
                public string task_name;
            }
        }
        [DataContract]
        private class QueryTaskResult
        {
            [DataMember]
            public Dictionary<string, Entry> task_info;
            [DataContract]
            public class Entry
            {
                [DataMember]
                public long file_size;
                [DataMember]
                public long finished_size;
                [DataMember]
                public int status;
            }
        }
        [DataContract]
        private class QueryTorrentResult
        {
            [DataMember]
            public TorrentInfo torrent_info;
            [DataContract]
            public class TorrentInfo
            {
                [DataMember]
                public QueryLinkResult.Entry[] file_info;
                [DataMember]
                public string sha1;
            }
        }
        [DataContract]
        private class QueryMagnetResult
        {
            [DataMember]
            public MagnetInfo magnet_info;
            [DataContract]
            public class MagnetInfo
            {
                [DataMember]
                public QueryLinkResult.Entry[] file_info;
            }
        }
    }
}
