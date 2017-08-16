using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.ClientA;
using System.IO;
using ZYSocket.share;

namespace testclient
{
    class Program
    {
        /// <summary>
        /// 建立一个SOCKET客户端
        /// </summary>
        public static SocketClient client = new SocketClient();

        static void Main(string[] args)
        {
            

            client.DataOn += new DataOn(client_DataOn); //数据包进入事件

            client.Disconnection += new ExceptionDisconnection(client_Disconnection); //数据包断开事件

            if (client.ConnectionTo(RConfig.ReadString("Host"), RConfig.ReadInt("Port"))) //使用同步连接到服务器，一步就用Begin开头的那个
            {
                while (true)
                {
                    //Console.ReadLine();



                    testClass.PPo temp = new testClass.PPo();
                    temp.Id = 1;
                    temp.Message = "通过对象通讯";
                    temp.guid = new List<Guid>();

                    for (int i = 0; i < 3000; i++)
                    {
                        temp.guid.Add(Guid.NewGuid());
                    }
                    client.SendTo(BufferFormat.FormatFCA(temp));  //讲一个PPO对象发送出去


                    // Console.ReadLine();

                    BufferFormat buffmat = new BufferFormat(1001);
                    buffmat.AddItem(2);
                    buffmat.AddItem("通过组合数据包通讯，GUID is object");
                    buffmat.AddItem(Guid.NewGuid());

                    client.SendTo(buffmat.Finish()); //用组合数据包模拟PPO对象

                   // Console.ReadLine();

                    BufferFormat buffmat2 = new BufferFormat(1002);
                    buffmat2.AddItem(3);
                    buffmat2.AddItem("通过组合数据包通讯 all buff");
                    buffmat2.AddItem(Guid.NewGuid().ToString());
                    client.SendTo(buffmat2.Finish()); //用组合数据包模拟PPO对象 但GUID 是字符串类型

                }

            }
            else
            {
                Console.WriteLine("无法连接服务器");
            }

            Console.ReadLine();
        }

        static void client_Disconnection(string message)
        {
            Console.WriteLine(message);
        }

        static void client_DataOn(byte[] Data)
        {
            Console.WriteLine(Data.Length);
        }
    }
}
