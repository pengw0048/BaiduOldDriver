using System;
using NetDisk;
using System.IO;
using System.Threading;

namespace GoodSamaritan
{
    class Program
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
        static Credential Login(string username, string password)
        {
            var checkResult = Authentication.LoginCheck(username);
            CheckSuccess(checkResult);
            if (checkResult.needVCode)
            {
                File.WriteAllBytes("vcode.png", checkResult.image);
                Console.WriteLine("Verification code required. Input the text in vcode.png.");
                checkResult.verifyCode = Console.ReadLine();
            }
            else Console.WriteLine("Verification code NOT required.");
            var loginResult = Authentication.Login(username, password, checkResult);
            CheckSuccess(loginResult);
            Console.WriteLine(loginResult.credential);
            Console.WriteLine("uid: " + loginResult.credential.uid);
            return loginResult.credential;
        }
        static void Main(string[] args)
        {
            int port;
            if (args.Length!=3||!int.TryParse(args[0], out port))
            {
                Console.WriteLine("Parameters: <port> <username> <password>");
                return;
            }
            var cred = Login(args[1], args[2]);
            HttpServer httpServer = new MyHttpServer(port, cred);
            var thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            Console.WriteLine("Listening on port " + port);
        }
    }
}
