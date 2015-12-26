using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPCConsoleClient
{
    public class ClientCall
    {
        public DateTime GetClientDateTime()
        {
            return DateTime.Now;
        }

        public long Add(long a, long b)
        {
            Console.WriteLine("服务器请求计算" + a + "+" + b + "=?");

            return a + b;
        }
    }
}
