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
            server.ReadOutTime = 800; //设置超时时间
            server.IsUseTaskQueue = true;
            server.MsgOut += Server_MsgOut;
            server.Start();
            Console.ReadLine();
        }

        private static void Server_MsgOut(string msg, MsgOutType logType)
        {
            Console.WriteLine(msg);
        }
    }
}
