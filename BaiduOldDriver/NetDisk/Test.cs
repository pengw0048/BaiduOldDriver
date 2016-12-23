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
            var checkResult = Authentication.LoginCheck(username);
            if (!checkResult.success)
            {
                Console.WriteLine(checkResult.exception);
                Console.ReadLine();
                return;
            }
            if (checkResult.needVCode)
            {
                File.WriteAllBytes("vcode.png", checkResult.image);
                Console.WriteLine("Verification code required.");
                checkResult.verifyCode = Console.ReadLine();
                try
                {
                    File.Delete("vcode.png");
                }
                catch (Exception) { }
            }
            else Console.WriteLine("Verification code NOT required.");
            var loginResult = Authentication.Login(username, password, checkResult);
            if (!loginResult.success)
            {
                Console.WriteLine(loginResult.exception);
                Console.ReadLine();
                return;
            }
            Console.WriteLine(loginResult.credential);
            Console.ReadLine();
        }
    }
}
