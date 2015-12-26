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
                client.Disconn += Client_Disconn;
                client.RegModule(new ClientCall());

                if (client.Call<ServerClass, bool>(p => p.LogOn("my is test", "123123")))
                {
                    Console.WriteLine("LogOn Is OK");

                    while (true)
                    {
                        Console.ReadLine();

                        DateTime time = client.Call<ServerClass, DateTime>(p => p.GetServerTime());

                        Console.WriteLine("Serve time is " + time);

                        int value = 0;

                        client.Call<ServerClass>(p => p.OutRandom(out value));

                        Console.Write("Random value is " + value);

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
