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

                var server = client.GetRPC<ServerClass>();

                if (server.LogOn(user, "123123"))
                {
                    
                    Console.WriteLine("LogOn Is OK");

                    while (true)
                    {
                        string msg = Console.ReadLine();

                        server.SendAll(msg);


                        DateTime time = server.GetServerTime();

                        Console.WriteLine("Serve time is " + time);

                        int value = 0;

                        client.AsynCall(()=> server.OutRandom(out value)).Wait();

                        Console.WriteLine("Random value is " + value);

                        Data x = new Data()
                        {
                            Name = "II",
                            Value = 0
                        };


                        var v = server.Return(x);

                        Console.WriteLine("Data Name " + v.Name);

                        var l = server.RecComputer(10); //这叫递归吗？ 代价太大，深度最好别超过5层 实在没办法记得设置outtime

                        Console.WriteLine("Rec computer value:" + l);

                    

                        var ary = server.array(new string[] { "123", "123" }); //Array + string


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
