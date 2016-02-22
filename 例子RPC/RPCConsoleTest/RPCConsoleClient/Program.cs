using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.RPCX.Client;

namespace RPCConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {

            LogAction.LogOut += LogAction_LogOut;
            RPCClient client = new RPCClient();
            if (client.Connection("127.0.0.1", 9952))
            {
                client.OutTime = 8000;
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

                        server.OutRandom(out value);

                        Console.WriteLine("Random value is " + value);

                        Data x = new Data()
                        {
                            Name = "II",
                            Value = 0
                        };


                        var v = server.Return(x);

                        Console.WriteLine("Data Name " + v.Name);                        

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

                }else
                {
                    Console.WriteLine("password error");
                    Console.ReadLine();
                }

            }

        }

        private static void Client_Disconn(string message)
        {
            Console.WriteLine(message);
        }

        private static void LogAction_LogOut(string msg, LogType type)
        {
            Console.WriteLine(msg);
        }

       
    }
}
