using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Client;

namespace RPCConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {


            RPCClient client = new RPCClient();
            if (client.Connection("127.0.0.1", 9952))
            {
                client.OutTime = 2000;
                client.Disconn += Client_Disconn;
                client.RegModule(new ClientCall());

                Console.WriteLine("input userName:");
                string user = Console.ReadLine();
                if (client.Call<ServerClass, bool>(p => p.LogOn(user, "123123")))
                {
                    Console.WriteLine("LogOn Is OK");

                    while (true)
                    {
                        string msg= Console.ReadLine();

                        client.Call<ServerClass>(p => p.SendAll(msg));


                        DateTime time = client.Call<ServerClass, DateTime>(p => p.GetServerTime());

                        Console.WriteLine("Serve time is " + time);

                        int value = 0;

                        client.Call<ServerClass>(p => p.OutRandom(out value));

                        Console.WriteLine("Random value is " + value);

                        Data x = new Data()
                        {
                           Name="II"
                        };

                        var v=  client.Call<ServerClass,Data>(p => p.Return(x));

                        Console.WriteLine("Data Name "+v.Name);

                        var l = client.Call<ServerClass, int>(p => p.RecComputer(10)); //这叫递归吗？ 代价太大，深度最好别超过5层 实在没办法记得设置outtime

                        Console.WriteLine("Rec computer value:" + l);

                    }

                }

            }

        }

        private static void Client_Disconn(string message)
        {
            Console.WriteLine(message);
        }
    }
}
