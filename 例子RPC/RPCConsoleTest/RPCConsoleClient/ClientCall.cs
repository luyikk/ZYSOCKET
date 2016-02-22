using System;
using System.Collections.Generic;

using System.Text;
using ZYSocket.RPCX.Client;

namespace RPCConsoleClient
{
    
    public class ClientCall
    {
        [RPCMethod]
        public DateTime GetClientDateTime()
        {
            return DateTime.Now;
        }

        [RPCMethod]
        public long Add(long a, long b)
        {
            Console.WriteLine("服务器请求计算" + a + "+" + b + "=?");
           
            return a + b;
        }

        [RPCMethod]
        public void ShowMsg(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
