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
                client.OutTime = 200000;
                client.Disconn += Client_Disconn;
                client.RegModule(new ClientCall());

                Console.WriteLine("input userName:");
                string user = Console.ReadLine();

                if (client.Call<ServerClass, bool>(p => p.LogOn(user, "123123")))
                {
                    
                    Console.WriteLine("LogOn Is OK");

                    while (true)
                    {
                        string msg = Console.ReadLine();

                        //client.Call<ServerClass>(p => p.SendAll(msg));


                        DateTime time = client.Call<ServerClass, DateTime>(p => p.GetServerTime());

                        Console.WriteLine("Serve time is " + time);

                        int value = 0;

                        client.Call<ServerClass>(p => p.OutRandom(out value));

                        Console.WriteLine("Random value is " + value);

                        Data x = new Data()
                        {
                            Name = "II",
                            Value = 0
                        };


                        var v = client.Call<ServerClass, Data>(p => p.Return(x));

                        Console.WriteLine("Data Name " + v.Name);

                        var l = client.Call<ServerClass, int>(p => p.RecComputer(10)); //这叫递归吗？ 代价太大，深度最好别超过5层 实在没办法记得设置outtime

                        Console.WriteLine("Rec computer value:" + l);

                        var server = client.GetRPC<ServerClass>();

                        var ary = server.array(new string[] { "123", "123" }); //Array + string

                        foreach (var item in ary)
                        {
                            Console.WriteLine(item);
                        }

                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                        stop.Start();
                        //int j = 0;
                        for (int i = 0; i < 10000; i++)
                        {
                            x = server.Return(x);
                        }
                        stop.Stop();
                        Console.WriteLine("Time:" + stop.ElapsedMilliseconds + " J:" + x.Value);


                        Console.ReadLine();

                        int mm = 0;
                        int xx = 1;
                        stop.Reset();
                        stop.Start();
                        //int j = 0;
                        for (int i = 0; i < 10000; i++)
                        {
                            server.TestOutAndRef(out mm, ref xx);
                        }
                        stop.Stop();
                        Console.WriteLine("Time:" + stop.ElapsedMilliseconds + " mm:" + mm + " xx:" + xx);




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
