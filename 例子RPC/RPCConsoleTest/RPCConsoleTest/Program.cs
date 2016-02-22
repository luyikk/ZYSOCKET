using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPCX.Service;

namespace RPCConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LogAction.LogOut += LogAction_LogOut;
            RPCServer server = new RPCServer();
            server.RegServiceModule(new ServerClass());
            server.ReadOutTime = 8000; //设置超时时间
            
            server.IsUseTask = true; //为了使用递归函数。 C1->S-->C1-->S 并且是同步访问
            server.IsCallReturn = true;
            server.Start();
            Console.ReadLine();
        }

        private static void LogAction_LogOut(string msg, LogType type)
        {
            Console.WriteLine(msg);
        }

               
    }
}
