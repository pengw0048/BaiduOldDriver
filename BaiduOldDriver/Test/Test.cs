using System;
using System.Linq;
using NetDisk;
using Downloader;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Test
{
    public class Test
    {
        static void CheckSuccess(Result result)
        {
            if (!result.success)
            {
                Console.WriteLine(result.exception);
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }
        static void Main(string[] args)
        {
            var username = "伪红学家";
            var password = "******";
            // Test login check
            var checkResult = Authentication.LoginCheck(username);
            CheckSuccess(checkResult);
            // Handle verify code
            if (checkResult.needVCode)
            {
                File.WriteAllBytes("vcode.png", checkResult.image);
                Console.WriteLine("Verification code required. Input the text in vcode.png.");
                try
                {
                    System.Diagnostics.Process.Start("vcode.png");
                }
                catch (Exception) { }
                checkResult.verifyCode = Console.ReadLine();
                try
                {
                    File.Delete("vcode.png");
                }
                catch (Exception) { }
            }
            else Console.WriteLine("Verification code NOT required.");
            // Test login
            var loginResult = Authentication.Login(username, password, checkResult);
            CheckSuccess(loginResult);
            Console.WriteLine(loginResult.credential);
            Console.WriteLine("uid: " + loginResult.credential.uid);
            var credential = loginResult.credential;
            // Test get quota
            var quotaResult = Operation.GetQuota(credential);
            CheckSuccess(quotaResult);
            Console.WriteLine(quotaResult.used + "/" + quotaResult.total);
            // Test get user info
            var infoResult = Operation.GetUserInfo(credential);
            CheckSuccess(infoResult);
            Console.WriteLine(infoResult.records[0].uname + " " + infoResult.records[0].priority_name + " " + infoResult.records[0].avatar_url);
            // Test list file
            var fileListResult = Operation.GetFileList("/", credential);
            CheckSuccess(fileListResult);
            Console.WriteLine(string.Join("\r\n", fileListResult.list.Take(5).Select(e => e.path + " " + e.isdir + " " + e.size).ToArray()));
            // Test thumbnail
            var thumbnailResult = Operation.GetThumbnail("/1.mp4", credential);
            CheckSuccess(thumbnailResult);
            Console.WriteLine("Thumbnail " + thumbnailResult.image.Length + " bytes.");
            try
            {
                File.WriteAllBytes("thumb.jpg", thumbnailResult.image);
                var process = System.Diagnostics.Process.Start("thumb.jpg");
            }
            catch (Exception) { }
            // Test rapid upload
            var rapidUploadResult = Operation.RapidUpload(UploadHelper.GetFileProperty("Z:\\Thunder9.0.14.358.exe"), "/t.exe", credential);
            CheckSuccess(rapidUploadResult);
            Console.WriteLine("Rapid: " + rapidUploadResult.info.path + " " + rapidUploadResult.info.size);
            // Test simple upload
            var simpleUploadResult = Operation.SimpleUpload("Z:\\1.rar", "/1.rar", credential, "c.pcs.baidu.com");
            CheckSuccess(simpleUploadResult);
            Console.WriteLine(simpleUploadResult.path + " " + simpleUploadResult.size);
            // Test get download urls
            var downloadResult = Operation.GetDownload("/1.mp4", credential);
            CheckSuccess(downloadResult);
            Console.WriteLine("Download URL count = " + downloadResult.urls.Length + " " + downloadResult.urls[0].rank + " " + downloadResult.urls[0].url);
            // Test file operations
            var fileopResult = Operation.Copy("/1.mp4", "/", "2.mp4", credential);
            CheckSuccess(fileopResult);
            fileopResult = Operation.Move("/2.mp4", "/", "3.mp4", credential);
            CheckSuccess(fileopResult);
            fileopResult = Operation.Rename("/3.mp4", "4.mp4", credential);
            CheckSuccess(fileopResult);
            fileopResult = Operation.Delete("/4.mp4", credential);
            CheckSuccess(fileopResult);
            fileopResult = Operation.CreateFolder("/test", credential);
            CheckSuccess(fileopResult);
            Console.WriteLine("New folder name: " + fileopResult.path);
            // Test share
            var shareResult = Operation.Share(new[] { "/1.mp4" }, credential);
            CheckSuccess(shareResult);
            Console.WriteLine(shareResult.link + " " + shareResult.shorturl);
            shareResult = Operation.Share(new[] { "/1.mp4" }, credential, "8888");
            CheckSuccess(shareResult);
            Console.WriteLine(shareResult.link + " " + shareResult.shorturl);
            // Test transfer shared files
            var transferResult = Operation.Transfer("http://pan.baidu.com/s/1hsfZ1TM", "/", credential, "1w9w");
            CheckSuccess(transferResult);
            Console.WriteLine(transferResult.extra.list.Select(e => e.from + " " + e.to + "\r\n").ToString());
            // Test offline
            var queryLinkResult = Operation.QueryLinkFiles("/downfile.torrent", credential);
            CheckSuccess(queryLinkResult);
            queryLinkResult.files.ToList().ForEach(f => Console.WriteLine(f.file_name + " " + f.size));
            var addOfflineResult = Operation.AddOfflineTask("/downfile.torrent", "/", credential, new[] { 1 }, queryLinkResult.sha1);
            CheckSuccess(addOfflineResult);
            Console.WriteLine(addOfflineResult.task_id + " " + addOfflineResult.rapid_download);
            addOfflineResult = Operation.AddOfflineTask("http://www.baidu.com/", "/", credential);
            CheckSuccess(addOfflineResult);
            Console.WriteLine(addOfflineResult.task_id + " " + addOfflineResult.rapid_download);
            addOfflineResult = Operation.AddOfflineTask("magnet:?xt=urn:btih:eb748516ee0968422d9827a9991d28cbd4dc4f3f", "/", credential, new[] { 1, 2 });
            CheckSuccess(addOfflineResult);
            Console.WriteLine(addOfflineResult.task_id + " " + addOfflineResult.rapid_download);
            addOfflineResult = Operation.AddOfflineTask("ed2k://|file|[柳井正与优衣库].蔡成平.文字版(ED2000.COM).epub|890525|90a92b89df20a8b2283fe3450f872590|h=oxrt54bznbtq5pr47dfmj4zfn5ctbvek|/", "/", credential);
            CheckSuccess(addOfflineResult);
            Console.WriteLine(addOfflineResult.task_id + " " + addOfflineResult.rapid_download);
            var offlineListResult = Operation.GetOfflineList(credential);
            CheckSuccess(offlineListResult);
            offlineListResult.tasks.ToList().ForEach(t => Console.WriteLine(t.status + " " + t.task_id + " " + t.task_name + " " + t.finished_size + "/" + t.file_size + " " + t.od_type));
            Operation.ClearOfflineTask(credential);
            foreach (var item in offlineListResult.tasks)
            {
                Operation.CancelOfflineTask(item.task_id, credential);
                Operation.DeleteOfflineTask(item.task_id, credential);
            }
            offlineListResult = Operation.GetOfflineList(credential);
            CheckSuccess(offlineListResult);
            Debug.Assert(offlineListResult.tasks.Length == 0);
            Console.WriteLine("Offline list cleared.");
            // Test download
            var adapter = new BaiduAdapter() { credential = credential, path = "/1.mp4", size = fileListResult.list.First(e => e.path == "/1.mp4").size };
            var task = new FileTask(adapter, "Z:\\1.mp4");
            var st = new Stopwatch();
            st.Start();
            while (!task.IsCompleted())
            {
                Console.WriteLine(task.GetDownloadedBytes());
                foreach (var item in task.DownloadSources)
                {
                    Console.WriteLine("  " + item.URL.Substring(0,20) + " " + item.GetDownloadedBytes());
                }
                Thread.Sleep(1000);
            }
            st.Stop();
            Console.WriteLine("Time taken: " + st.ElapsedMilliseconds / 1000);
            // Done
            Console.WriteLine("Success");
            Console.ReadLine();
        }
    }
}
