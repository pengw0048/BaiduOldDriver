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
    [DataContract]
    public class ThumbnailResult : Result
    {
        [DataMember]
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
