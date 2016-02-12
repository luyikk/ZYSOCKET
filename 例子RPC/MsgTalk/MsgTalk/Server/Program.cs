using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Server;
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            RPCServer server = new RPCServer("127.0.0.1", 3000, 4000, 1024 * 64);
            server.RegServiceModule(new TalkService());
            server.MsgOut += Server_MsgOut;
            server.IsUseTaskQueue = false; //如果不搞 C1-->S--->C1 并且是同步访问的 就不要设置为TRUE
            server.Start();
            Console.ReadLine();
        }

        private static void Server_MsgOut(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
