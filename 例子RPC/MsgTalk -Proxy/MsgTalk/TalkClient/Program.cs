using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Client;

namespace TalkClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RPCClient client = new RPCClient();
            if (client.Connection("127.0.0.1", 3000))
            {
                client.OutTime = 10000000;
                client.Disconn += Client_Disconn;
                client.MsgOut += Client_MsgOut;
                client.RegModule(new Client());
                Console.Write("输入你的昵称:");

                var IServer = client.GetRPC<TalkService>();

                if (IServer.IsLogIn(Console.ReadLine()))
                {
                    while (true)
                    {                       
                        string msg = Console.ReadLine();
                        IServer.SendALL(msg);
                    
                    }
                }
            }
        }

        private static void Client_MsgOut(string msg)
        {
            Console.WriteLine(msg);
        }

        private static void Client_Disconn(string message)
        {
            Console.WriteLine(message);
        }
    }
}
