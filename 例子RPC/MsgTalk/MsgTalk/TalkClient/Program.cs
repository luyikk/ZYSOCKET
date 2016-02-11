using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Client;
using System.Threading.Tasks;

namespace TalkClient
{
    class Program
    {
        static void Main(string[] args)
        {
           
            RPCClient client = new RPCClient();
            if (client.Connection("127.0.0.1", 3000))
            {
                client.OutTime = 10000;
                client.Disconn += Client_Disconn;
                client.MsgOut += Client_MsgOut;
                client.RegModule(new Client());
                Console.Write("输入你的昵称:");

                var service = client.GetRPC<TalkService>();

                if (client.AsynCall<bool>(() => service.IsLogIn(Console.ReadLine())).Result)
                {
                    while (true)
                    {                       
                        string msg = Console.ReadLine();


                        Task<int>[] tasklist = new Task<int>[10000];

                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                        stop.Start();

                         int j = 0;
                        
                      
                        for (int i = 0; i < 10000; i++)
                        {
                            tasklist[i] = client.AsynCall<int>(() => service.value(i, j));
                          
                        }


                        Task.WaitAll(tasklist);

                        stop.Stop();


                        Console.WriteLine(stop.ElapsedMilliseconds);



                        client.AsynCall(() => service.SendALL(Console.ReadLine())).Wait();
                      
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
