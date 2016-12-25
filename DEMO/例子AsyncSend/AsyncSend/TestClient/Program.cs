using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ZYSocket.ClientA;
using ZYSocket.share;
namespace TestClient
{
    class Program
    {
        static ZYNetRingBufferPool Stream = new ZYNetRingBufferPool();
        static long count = 0;
        static System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
        static userinfo user;
        static void Main(string[] args)
        {
            SocketClient client = new SocketClient();
            if (client.ConnectionTo("127.0.0.1", 5566))
            {
                client.DataOn += client_BinaryInput;
                user = new userinfo(client.sock);
                //int i = 1;
                while (true)
                {
                    Console.ReadLine();
                    stop.Reset();
                    stop.Start();
                    count = 0;
                    for (int i = 0; i < 100000; i++)
                    {
                        BufferFormat buffer = new BufferFormat(1000);
                        buffer.AddItem(i.ToString());
                        buffer.AddItem(new byte[64]);
                        byte[] data = buffer.Finish();
                        client.SendTo(user,data);
                        //System.Threading.Thread.Sleep(1);
                    }
                    //i++;



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

                            count++;

                            if (count >= 100000)
                            {
                                stop.Stop();
                                Console.WriteLine(count + ":" + msg+":"+stop.ElapsedMilliseconds);
                            }

                        }
                        break;
                }

            }

        }


    }

    class userinfo : ZYSocket.ClientA.AsyncSend
    {
        public userinfo(Socket sock) : base(sock)
        {

        }

    }
}
