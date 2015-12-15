using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
using System.Threading.Tasks;
using System.Threading;

namespace ZYNetRingBufferStream
{
    class Program
    {
        static ZYNetRingBufferPoolV2 BufferPool = new ZYNetRingBufferPoolV2(128); //128Bit
        static void Main(string[] args)
        {
          

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    byte[] datax;
                    while (BufferPool.Read(out datax))
                    {
                        DataOn(datax); //打印数据包内容
                    }

                }

            });

            for (int i = 0; i < 20000; i++)
            {

                BufferFormatV2 buff1 = new BufferFormatV2(i);
                buff1.AddItem("HHHHHHHHHHH");

                byte[] data = buff1.Finish();


                byte[] pdata = new byte[1024];

                Buffer.BlockCopy(data, 0, pdata, 500, data.Length);


                //// BufferPool.Write(data);
                //foreach (var item in data)
                //{
                //    BufferPool.Write(new byte[] { item });
                //}

                BufferPool.Write(pdata, 500, data.Length);

               System.Threading.Thread.Sleep(1);

            }


            Console.WriteLine("Full Count:" + Lengt);
            Console.ReadLine();
        }

        static int Lengt = 0;

        static void DataOn(byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;

            if (read.ReadInt32(out lengt))
            {
                string msg;
                int cmd;
                if (read.ReadInt32(out cmd))
                {
                    if (read.ReadString(out msg))
                    {
                        Lengt++;
                        Console.WriteLine(lengt + ":" + cmd + "->" + msg);
                    }
                }
            }

        }
    }
}
