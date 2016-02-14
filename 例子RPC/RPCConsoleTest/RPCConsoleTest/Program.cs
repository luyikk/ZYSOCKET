using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Server;

namespace RPCConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RPCServer server = new RPCServer();
            server.RegServiceModule(new ServerClass());
            server.ReadOutTime = 200000; //设置超时时间
            server.MsgOut += Server_MsgOut;
            server.IsUseTaskQueue = true; //为了使用递归函数。 C1->S-->C1-->S 并且是同步访问
            server.Start();
            Console.ReadLine();
        }

        private static void Server_MsgOut(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
