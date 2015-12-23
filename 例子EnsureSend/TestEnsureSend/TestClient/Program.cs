using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.ClientB;
using ZYSocket.share;
namespace TestClient
{
    class Program
    {
        static ZYNetRingBufferPool Stream = new ZYNetRingBufferPool();


        static void Main(string[] args)
        {
            SocketClient client = new SocketClient();
            if (client.Connect("127.0.0.1", 5566))
            {
                client.BinaryInput += client_BinaryInput;
                client.StartRead();
                while (true)
                {
                    Console.ReadLine();


                    //for (int i = 0; i < 100000; i++)
                    //{
                        BufferFormat buffer = new BufferFormat(1000);
                        buffer.AddItem(1.ToString());
                        buffer.AddItem(new byte[64]);
                        byte[] data = buffer.Finish();
                        client.Send(data);
                        System.Threading.Thread.Sleep(1);
                  // }
                  



                }            

            }
        }

        static void client_BinaryInput(byte[] data)
        {
            Stream.Write(data);

            byte[] pdata;
            while (Stream.Read(out pdata))
            {
                DataOn(pdata);
            }
        }

        static void DataOn(byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int lengt = read.ReadInt32();
           

            if (lengt == read.Length)
            {
                int cmd = read.ReadInt32();

                switch (cmd)
                {
                    case 1000:
                        {
                            string msg = read.ReadString();

                            Console.WriteLine(msg);

                        }
                        break;
                }

            }

        }
    }
}
