using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetDisk
{
    public static class UploadHelper
    {
        public static string[] GetFileBlockMD5(string path)
        {
            return null;
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
}
