using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetDisk;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Connoisseur
{
    class Program
    {
        class Link
        {
            public string hash;
            public string name;
        }
        static void CheckSuccess(Result result)
        {
            if (!result.success)
            {
                Console.WriteLine(result.exception);
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }
        static Credential Login()
        {
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();
            var checkResult = Authentication.LoginCheck(username);
            CheckSuccess(checkResult);
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
            }
            else Console.WriteLine("Verification code NOT required.");
            var loginResult = Authentication.Login(username, password, checkResult);
            CheckSuccess(loginResult);
            return loginResult.credential;
        }
        static string GetName(string str)
        {
            try
            {
                var match = Regex.Match(str, "decodeURIComponent\\((.*?)\\);<");
                if (!match.Success) return null;
                var esc = string.Concat(match.Groups[1].Value.Split('+').Select(t => t.Replace("\"", "")));
                return Uri.UnescapeDataString(esc).Replace("<b>", "").Replace("</b>", "");
            }
            catch (Exception)
            {
                return null;
            }
        }
        static List<Link> Search(string keyword)
        {
            var ret = new List<Link>();
            var wc = new WebClient();
            Console.Write("Looking at search result page");
            for (int i = 1; i <= 100; i++)
            {
                var url = "http://www.btmeet.org/search/" + Uri.EscapeDataString(keyword) + "/" + i + "-1.html";
                Console.Write(" " + i);
                try
                {
                    var str = wc.DownloadString(url);
                    var matches = Regex.Matches(str, "a href=\"\\/wiki\\/([0-9a-f]+?).html(.*?)<\\/a>");
                    if (matches.Count == 0) break;
                    foreach (Match match in matches)
                    {
                        var name = GetName(match.Groups[2].Value);
                        ret.Add(new Link() { hash = match.Groups[1].Value, name = name == null ? match.Groups[1].Value : name });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            wc.Dispose();
            Console.WriteLine();
            return ret;
        }
        static void Offline(List<Link> links, string path, Credential credential)
        {
            var count = 0;
            foreach (var link in links)
            {
                Console.Write("Adding " + link.name + " ... ");
                var list = Operation.QueryLinkFiles("magnet:?xt=urn:btih:" + link.hash, credential);
                if (!list.success)
                {
                    Console.WriteLine("Query magnet link failed.");
                    continue;
                }
                var res = Operation.AddOfflineTask("magnet:?xt=urn:btih:" + link.hash, path, credential, Enumerable.Range(1, list.files.Length).ToArray());
                if (!res.success) Console.WriteLine("Add offline task failed.");
                else Console.WriteLine("Success with rapid = " + res.rapid_download);
                if (res.success && res.rapid_download == 0)
                {
                    count++;
                    if (count >= 5)
                    {
                        Console.Write("Too many concurrent tasks, wait 3 seconds ... ");
                        Thread.Sleep(3000);
                        var tasks = Operation.GetOfflineList(credential).tasks.Where(t => t.status != 0).ToArray();
                        if (tasks.Length > 0)
                        {
                            Console.Write("Abort incomplete tasks ... ");
                            foreach (var task in tasks)
                            {
                                Operation.CancelOfflineTask(task.task_id, credential);
                            }
                        }
                        count = 0;
                        Console.WriteLine("Okay, move on.");
                    }
                }
            }
        }
        static void CleanUp(Credential credential)
        {
            var tasks = Operation.GetOfflineList(credential);
            foreach (var task in tasks.tasks.Where(t => t.status != 0))
            {
                Operation.CancelOfflineTask(task.task_id, credential);
            }
            Console.WriteLine(tasks.tasks.Count(t => t.status == 0) + " completed tasks.");
            Operation.ClearOfflineTask(credential);
        }
        static void Main(string[] args)
        {
            var credential = Login();
            Console.WriteLine("Login OK, uid = " + credential.uid);
            Console.Write("Keyword: ");
            var keyword = Console.ReadLine();
            Console.WriteLine("Searching magnet links ...");
            var links = Search(keyword);
            Console.WriteLine("Search complete with " + links.Count + " results.");
            var folder = Operation.CreateFolder("/" + keyword, credential);
            CheckSuccess(folder);
            Console.WriteLine("Files will be saved to " + folder.path);
            Offline(links, folder.path, credential);
            Console.WriteLine("Cleaning up ...");
            CleanUp(credential);
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
