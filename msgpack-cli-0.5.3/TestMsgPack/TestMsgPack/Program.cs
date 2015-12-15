using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack.Serialization;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ZYSocket.share;


namespace TestMsgPack
{
    class Program
    {
        /// <summary>
        /// 预热用
        /// </summary>
        static void Run()
        {

          
            SerializationContext.Default.GetSerializer<B>();

            B tmp = new B();
            tmp.Msg = "你好ˇ⒕,叁,IV,伍Α,ΘǚΔ♂Θ∏★☆ウ∞";
            tmp.X = 111;
            tmp.Res = 555;
            tmp.time = DateTime.Now;
         
            byte[] array = SerializationContext.Default.GetSerializer<B>().PackSingleObject(tmp);
            B T1= SerializationContext.Default.GetSerializer<B>().UnpackSingleObject(array);


            string a = T1.time.Kind.ToString();

                      


            Console.ReadLine();


            //BufferFormatV2 buff = new BufferFormatV2(1000);
            //buff.AddItem("你好");
            //buff.AddItem(111);
            //buff.AddItem(555);
            //byte[] data = buff.Finish();

            //ReadBytesV2 read = new ReadBytesV2(data);

            //int lengt;
            //int cmd;
            //string msg;
            //int i1;
            //int i2;

            //if (read.ReadInt32(out lengt) && read.ReadInt32(out cmd) && read.ReadString(out msg) && read.ReadInt32(out i1) && read.ReadInt32(out i2))
            //{

            //    B var2 = new B()
            //    {
            //        Msg = msg,
            //        X = i1,
            //        Res = i2
            //    };

            //}


            //byte[] xy = BufferFormatV2.FormatFCA(tmp);

            //ReadBytesV2 buff2=new ReadBytesV2(xy);

            //if (buff2.ReadInt32(out lengt) && buff2.ReadInt32(out cmd))
            //{
            //    B vx3;

            //    if (buff2.ReadObject<B>(out vx3))
            //    {

            //    }
            //}
        }



        static void Test(object p)
        {
            byte[] array = SerializationContext.Default.GetSerializer(p.GetType(),p).PackSingleObject(p);
        }

        static void Main(string[] args)
        {

            //---预热--
            Run();
            //-----
            
       

            //B tmp2 = new B();
            //tmp2.Msg = "你好2";
            //tmp2.X = 1111;
            //tmp2.Res = 5551;

            //Test(tmp2);

            //Stopwatch test = new System.Diagnostics.Stopwatch();
            //test.Start();




            //byte[] array = SerializationContext.Default.GetSerializer<B>().PackSingleObject(tmp2);

            //B var = SerializationContext.Default.GetSerializer<B>().UnpackSingleObject(array);


            //test.Stop();

            //Console.WriteLine(test.ElapsedTicks);


            ////-----------顺序读位---------

            //test.Reset();
            //test.Start();


            //BufferFormatV2 buff = new BufferFormatV2(1000);
            //buff.AddItem("你好");
            //buff.AddItem(111);
            //buff.AddItem(555);
            //byte[] data = buff.Finish();

            //ReadBytesV2 read = new ReadBytesV2(data);

            //int lengt;
            //int cmd;
            //string msg;
            //int i1;
            //int i2;

            //if (read.ReadInt32(out lengt) && read.ReadInt32(out cmd) && read.ReadString(out msg) && read.ReadInt32(out i1) && read.ReadInt32(out i2))
            //{

            //    B var2 = new B()
            //    {
            //        Msg = msg,
            //        X = i1,
            //        Res = i2
            //    };

            //}


            //test.Stop();

            //Console.WriteLine(test.ElapsedTicks);

            //test.Reset();
            //test.Start();

            //byte[] xy = BufferFormatV2.FormatFCA(tmp2);

            //ReadBytesV2 buff2 = new ReadBytesV2(xy);

            //if (buff2.ReadInt32(out lengt) && buff2.ReadInt32(out cmd))
            //{
            //    B vx3;

            //    if (buff2.ReadObject<B>(out vx3))
            //    {

            //    }
            //}

            //test.Stop();

            //Console.WriteLine(test.ElapsedTicks);


            //Console.ReadLine();








            //var serial = MessagePackSerializer.Get<UserInfo>();
            //var memStream = new MemoryStream();
            //var stopWatch = new Stopwatch();

            //var userInfo = new UserInfo()
            //{
            //    Age = 881,
            //    Age1 = 882,
            //    Age2 = 883,
            //    Age3 = 884,
            //    Age4 = 885,
            //    NickName = "昵称5",
            //    Gender = "男",
            //    Location = "最近一段时间以来，mina很火，和移动开发一样，异常的火爆。aaa前面写了几篇移动开发的文章，都还不错，你们的鼓励就是我最大的动力。好了，废话少说。我们来看下tcp通讯吧。nn",
            //};

            //var userInfo2 = new UserInfo2()
            //{
            //    Age = 881,
            //    //Age1 = 882,
            //    //  Age2 = 883,
            //    // Age3 = 884,
            //    //  Age4 = 885,
            //    //  NickName = "昵称5",
            //    //   Gender = "男a",
            //    //Location = "最近一段时间以来，mina很火，和移动开发一样，异常的火爆。aaa前面写了几篇移动开发的文章，都还不错，你们的鼓励就是我最大的动力。好了，废话少说。我们来看下tcp通讯吧。nn",
            //};

            //for (var i = 0; i < 1; i++)
            //{
            //    memStream.Position = 0;
            //    Serializer.Serialize<UserInfo2>(memStream, userInfo2);

            //    memStream.Position = 0;
            //    var temp1 = Serializer.Deserialize<UserInfo2>(memStream);

            //    memStream.Position = 0;
            //    var temp = Serializer.Deserialize<UserInfo>(memStream);
            //}

            //for (var i = 0; i < 1; i++)
            //{
            //    memStream.Position = 0;
            //    MessagePackSerializer.Get<UserInfo2>().Pack(memStream, userInfo2);

            //    memStream.Position = 0;
            //    MessagePackSerializer.Get<UserInfo2>().Unpack(memStream);

            //    memStream.Position = 0;

            //    MessagePackSerializer.Get(typeof(UserInfo)).Unpack(memStream);



            //    var temp = MessagePackSerializer.Get<UserInfo>().Unpack(memStream);
            //}

            //while (true)
            //{
            //    stopWatch.Restart();
            //    for (var i = 0; i < 100000; i++)
            //    {
            //        memStream.Position = 0;
            //        serial.Pack(memStream, userInfo);

            //        memStream.Position = 0;
            //        serial.Unpack(memStream);
            //    }
            //    Console.WriteLine("MsgPack:" + stopWatch.ElapsedMilliseconds);


            //    stopWatch.Restart();
            //    for (var i = 0; i < 100000; i++)
            //    {
            //        memStream.Position = 0;
            //        Serializer.Serialize<UserInfo>(memStream, userInfo);

            //        memStream.Position = 0;
            //        Serializer.Deserialize<UserInfo>(memStream);
            //    }
            //    Console.WriteLine("ProtoBuf:" + stopWatch.ElapsedMilliseconds);
            //}

            //Console.Read();
        }
    }
}