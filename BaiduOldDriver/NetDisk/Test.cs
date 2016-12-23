using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetDisk
{
    public class Test
    {
        static void Main(string[] args)
        {
            var username = "伪红学家";
            var password = "******";
            // Test login check
            var checkResult = Authentication.LoginCheck(username);
            if (!checkResult.success)
            {
                Console.WriteLine(checkResult.exception);
                Console.ReadLine();
                return;
            }
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
            if (!loginResult.success)
            {
                Console.WriteLine(loginResult.exception);
                Console.ReadLine();
                return;
            }
            Console.WriteLine(loginResult.credential);
            Console.WriteLine("uid: " + loginResult.credential.uid);
            var credential = loginResult.credential;
            // Test get quota
            var quotaResult = Operations.GetQuota(credential);
            if (!quotaResult.success)
            {
                Console.WriteLine(quotaResult.exception);
                Console.ReadLine();
                return;
            }
            Console.WriteLine(quotaResult.used + "/" + quotaResult.total);
            // Test get user info
            var infoResult = Operations.GetUserInfo(credential);
            if (!infoResult.success||infoResult.records.Length != 1)
            {
                Console.WriteLine(infoResult.exception);
                Console.ReadLine();
                return;
            }
            Console.WriteLine(infoResult.records[0].uname + " " + infoResult.records[0].priority_name + " " + infoResult.records[0].avatar_url);
            // Done
            Console.WriteLine("Success");
            Console.ReadLine();
        }
    }
}
