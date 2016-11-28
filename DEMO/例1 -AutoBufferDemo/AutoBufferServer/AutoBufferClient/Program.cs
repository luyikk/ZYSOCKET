using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;
using ZYSocket.ClientB;
using AutoBufferServer;

namespace AutoBufferClient
{
    class Program
    {
        static SocketClient Client;

        static ZYNetRingBufferPool bufferPool = new ZYNetRingBufferPool(1024 * 8);

        static ZYAutoBuffer autobuffer = new ZYAutoBuffer(typeof(Program));

        static bool IsLogOn;

        static void Main(string[] args)
        {
            Client = new SocketClient();

            if (Client.Connect("127.0.0.1", 9982))
            {
                Client.BinaryInput += Client_BinaryInput;
                Client.MessageInput += Client_MessageInput;
                Client.StartRead();

                Console.Write("UserName:");

                string username = Console.ReadLine();

                Console.Write("PassWord:");

                string password = Console.ReadLine();

                Client.Send(ZYAutoBuffer.Call(1000, username, password));


                while(true)
                {
                    string msg=  Console.ReadLine();

                    if(IsLogOn)
                    {
                        Client.Send(ZYAutoBuffer.Call(1001, msg));
                    }
                }

            }
            else
            {
                Console.WriteLine("not connect server");
            }

        }

        private static void Client_MessageInput(string message)
        {
            Console.WriteLine(message);
        }

        private static void Client_BinaryInput(byte[] data)
        {
            bufferPool.Write(data);

            byte[] pdata;

            while(bufferPool.Read(out pdata))
            {
                DataOn(pdata);
            }
        }

        private static void DataOn(byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int length;

            if(read.ReadInt32(out length)&&length==read.Length)
            {
                autobuffer.Run(read);
            }
        }

     
        [CmdTypeOfAttibutes(1000)]
        public static void IsLogOnTo(bool isOk)
        {
            if (isOk)
            {
                IsLogOn = true;
                Console.WriteLine("LogOn OK");
            }
            else
                Console.WriteLine("LogOn Fail");

        }

        [CmdTypeOfAttibutes(1001)]
        public static void MessageTo(Message msg)
        {
            Console.WriteLine("{0} {1}:{2}", msg.Date, msg.UserName, msg.Msg);
        }
    }
}
