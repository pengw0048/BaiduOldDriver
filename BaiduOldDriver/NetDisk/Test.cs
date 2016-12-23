using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDisk
{
    public class Test
    {
        static void Main(string[] args)
        {
            var res = Authentication.Login("伪红学家", "******");
            if (!res.success) Console.WriteLine(res.exception.Message);
            else Console.WriteLine(res.credential);
            Console.ReadLine();
        }
    }
}
