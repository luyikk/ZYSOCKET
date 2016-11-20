using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPCX.Client;
using System.Threading.Tasks;

namespace TalkClient
{
    class Program
    {
        static void Main(string[] args)
        {
            LogAction.LogOut += LogAction_LogOut;
            RPCClient client = new RPCClient();
            if (client.Connection("127.0.0.1", 3000))
            {
                client.OutTime = 10000;
                client.Disconn += Client_Disconn;               
                client.RegModule(new Client());
                Console.Write("输入你的昵称:");

                var service = client.GetRPC<TalkService>();

                if (service.IsLogIn(Console.ReadLine()))
                {
                    while (true)
                    {
                        //string msg = Console.ReadLine();



                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                        stop.Start();

                        int j = 0;

                        for (int i = 0; i < 10000; i++)
                        {
                            j = service.value(i, j);

                        }
                        stop.Stop();

                        Console.WriteLine("sync call ms:"+stop.ElapsedMilliseconds);

                        stop.Reset();
                        stop.Start();

                        j = 0;
                        Parallel.For(0, 10000, i =>
                          {
                              j = service.value(i, j);

                          });

                        stop.Stop();

                        Console.WriteLine("Parallel sync call ms:" + stop.ElapsedMilliseconds);

                        stop.Reset();
                        stop.Start();

                        Parallel.For(0, 10000, i =>
                        {
                           service.notReturn(i);
                        });

                        stop.Stop();

                        Console.WriteLine("Parallel not return async call ms:" + stop.ElapsedMilliseconds);
                        service.SendALL(Console.ReadLine());

                    }
                }
            }
        }

        private static void LogAction_LogOut(string msg, LogType type)
        {
            Console.WriteLine(msg);
        }


        private static void Client_Disconn(string message)
        {
            Console.WriteLine(message);
        }
    }
}
