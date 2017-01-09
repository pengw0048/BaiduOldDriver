using System;
using System.Net;

namespace NetDisk
{
    public class Result
    {
        public bool success;
        public Exception exception;
    }
    public class QuotaResult : Result
    {
        public int errno;
        public long total;
        public long free;
        public bool expire;
        public long used;
    }
    public class UserInfoResult : Result
    {
        public int errno;
        public Entry[] records;
        public class Entry
        {
            public string avatar_url;
            public string uname;
            public string priority_name;
        }
    }
    public class FileListResult : Result
    {
        public int errno;
        public Entry[] list;
        public class Entry
        {
            public int isdir;
            public string path;
            public string server_filename;
            public long size;
        }
    }
    public class ThumbnailResult : Result
    {
        public byte[] image;
    }
    public class GetDownloadResult : Result
    {
        public Entry[] urls;
        public class Entry
        {
            public int rank;
            public string url;
        }
    }
    public class FileOperationResult : Result
    {
        public int errno;
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
        public class Entry
        {
            public string file_name;
            public long size;
        }
    }
    public class AddOfflineTaskResult : Result
    {
        public int rapid_download;
        public long task_id;
    }
    public class ShareResult : Result
    {
        public int errno;
        public long shareid;
        public string link;
        public string shorturl;
    }
    public class TransferResult : Result
    {
        public int errno;
        public Extra extra;
        public class Extra
        {
            public Entry[] list;
            public class Entry
            {
                public string from;
                public string to;
            }
        }
    }
    public class InitUploadResult : Result
    {
        public int[] block_list;
        public int errno;
        public string uploadid;
    }
    public class CommitUploadResult : Result
    {
        public long ctime;
        public int errno;
        public long fs_id;
        public int isdir;
        public string md5;
        public long mtime;
        public string name;
        public string path;
        public long size;
    }
    public class RapidUploadResult : Result
    {
        public int errno;
        public FileListResult.Entry info;
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
    public class GetUploadServersResult: Result
    {
        public string[] servers;
    }
}
