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
                client.Disconn += Client_Disconn;
                client.MsgOut += Client_MsgOut;
                client.RegModule(new Client());
                Console.Write("输入你的昵称:");
                if (client.Call<TalkService, bool>(p => p.IsLogIn(Console.ReadLine())))
                {
                    while (true)
                    {                       
                        string msg = Console.ReadLine();
                        client.Call<TalkService>(p => p.SendALL(msg));
                      
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
