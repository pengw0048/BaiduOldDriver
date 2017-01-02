using System;
using System.Net;
using System.Runtime.Serialization;

namespace NetDisk
{
    [DataContract]
    public class Result
    {
        public bool success;
        public Exception exception;
    }
    [DataContract]
    public class QuotaResult : Result
    {
        [DataMember]
        public int errno;
        [DataMember]
        public long total;
        [DataMember]
        public long free;
        [DataMember]
        public bool expire;
        [DataMember]
        public long used;
    }
    [DataContract]
    public class UserInfoResult : Result
    {
        [DataMember]
        public int errno;
        [DataMember]
        public Entry[] records;
        [DataContract]
        public class Entry
        {
            [DataMember]
            public string avatar_url;
            [DataMember]
            public string uname;
            [DataMember]
            public string priority_name;
        }
    }
    [DataContract]
    public class FileListResult : Result
    {
        [DataMember]
        public int errno;
        [DataMember]
        public Entry[] list;
        [DataContract]
        public class Entry
        {
            [DataMember]
            public int isdir;
            [DataMember]
            public string path;
            [DataMember]
            public string server_filename;
            [DataMember]
            public long size;
        }
    }
    public class ThumbnailResult : Result
    {
        public byte[] image;
    }
    [DataContract]
    public class GetDownloadResult : Result
    {
        [DataMember]
        public Entry[] urls;
        [DataContract]
        public class Entry
        {
            [DataMember]
            public int rank;
            [DataMember]
            public string url;
        }
    }
    [DataContract]
    public class FileOperationResult : Result
    {
        [DataMember]
        public int errno;
        [DataMember]
        public string path;
    }
    public class OfflineListResult : Result
    {
        public Entry[] tasks;
        public class Entry
        {
            public long create_time;
            public int od_type;
            public string save_path;
            public string source_url;
            public long task_id;
            public string task_name;
            public long file_size;
            public long finished_size;
            public int status;
        }
    }
    public class QueryLinkResult : Result
    {
        public Entry[] files;
        public string sha1;
        [DataContract]
        public class Entry
        {
            [DataMember]
            public string file_name;
            [DataMember]
            public long size;
        }
    }
    [DataContract]
    public class AddOfflineTaskResult: Result
    {
        [DataMember]
        public int rapid_download;
        [DataMember]
        public long task_id;
    }
    [DataContract]
    public class ShareResult : Result
    {
        [DataMember]
        public int errno;
        [DataMember]
        public long shareid;
        [DataMember]
        public string link;
        [DataMember]
        public string shorturl;
    }
    public class LoginResult : Result
    {
        public Credential credential;
    }
    public class LoginCheckResult : Result
    {
        public bool needVCode;
        public string codeString;
        public string verifyCode;
        public byte[] image;
        public Cookie baiduid;
        public string ltoken;
    }
}
