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
            server.ReadOutTime = 2000000; //设置超时时间
            server.IsUseTaskQueue = true;
            server.Start();
            Console.ReadLine();
        }
    }
}
