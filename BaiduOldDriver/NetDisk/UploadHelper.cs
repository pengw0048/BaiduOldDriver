using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DamienG.Security.Cryptography;

namespace NetDisk
{
    public static class UploadHelper
    {
        public static FileProperty GetFileProperty(string path)
        {
            var ret = new FileProperty() { path = path };
            var info = new FileInfo(path);
            ret.size = info.Length;
            ret.mtime= (long)(info.LastAccessTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            ret.md5 = GetMD5HashFromFile(path);
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var md5 = new MD5CryptoServiceProvider();
                var arr = new byte[4 * 1024 * 1024];
                var len = fs.Read(arr, 0, 256 * 1024);
                ret.slice_md5 = ByteArrayToHexString(md5.ComputeHash(arr, 0, len));
                fs.Seek(0, SeekOrigin.Begin);
                var blocks = new List<string>();
                while (true)
                {
                    len = fs.Read(arr, 0, 4 * 1024 * 1024);
                    if (len <= 0) break;
                    blocks.Add(ByteArrayToHexString(md5.ComputeHash(arr, 0, len)));
                }
                ret.blocks = blocks.ToArray();
                fs.Seek(0, SeekOrigin.Begin);
                var crc32 = new Crc32();
                ret.crc32 = string.Empty;
                foreach (byte b in crc32.ComputeHash(fs)) ret.crc32 += b.ToString("x2").ToLower();
            }
            return ret;
        }
        public static string GetMD5HashFromFile(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                var md5 = new MD5CryptoServiceProvider();
                var retVal = md5.ComputeHash(file);
                return ByteArrayToHexString(retVal);
            }
        }
        private static string ByteArrayToHexString(byte[] arr)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                sb.Append(arr[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    public class FileProperty
    {
        public string path;
        public long size;
        public string md5;
        public string slice_md5;
        public string crc32;
        public long mtime;
        public string[] blocks;
    }
}
