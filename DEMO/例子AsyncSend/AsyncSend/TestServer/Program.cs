using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.Server;
using ZYSocket.share;

namespace TestServer
{
    class Program
    {

        static ZYSocketSuper server = new ZYSocketSuper();
       
        static void Main(string[] args)
        {
            server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
            server.IsOffsetInput = true;
            server.Start(); //启动服务器

            Console.ReadLine();
        }

        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());
            socketAsync.UserToken = new UserInfo(socketAsync);
            return true;
        }


        /// <summary>
        /// 用户断开代理（你可以根据socketAsync 读取到断开的
        /// </summary>
        /// <param name="message">断开消息</param>
        /// <param name="socketAsync">断开的SOCKET</param>
        /// <param name="erorr">错误的ID</param>
        static void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            Console.WriteLine(message);
            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
        }

        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        static void BinaryInputHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync)
        {

            try
            {

                //BuffList 数据包组合类 如果不想丢数据就用这个类吧
                UserInfo user = socketAsync.UserToken as UserInfo;


                user.Stream.Write(data, offset, count); //呵呵带offset的 可以省一次copy


                byte[] pdata;
                while (user.Stream.Read(out pdata))
                {
                    DataOn(pdata, user);
                }

            }
            catch (Exception er)
            {

            }
        }

        static void DataOn(byte[] data, UserInfo userinfo)
        {

            //建立一个读取数据包的类 参数是数据包
            //这个类的功能很强大,可以读取数据包的数据,并可以把你发送过来的对象数据,转换对象引用

            ReadBytes read = new ReadBytes(data);

            int lengt; //数据包长度,用于验证数据包的完整性
            int cmd; //数据包命令类型

            //注意这里一定要这样子写,这样子可以保证所有你要度的数据是完整的,如果读不出来 Raed方法会返回FALSE,从而避免了错误的数据导致崩溃
            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {  //read.Read系列函数是不会产生异常的

                //根据命令读取数据包
                switch (cmd)
                {
                    case 1000:
                        string msg = read.ReadString();

                        BufferFormat buffer = new BufferFormat(1000);
                        buffer.AddItem(msg);
                        buffer.AddItem(new byte[8096]);
                        byte[] pdata = buffer.Finish();
                        server.Send(userinfo, pdata);
                        //server.SendData(userinfo.Asyn.AcceptSocket, pdata);
                        break;


                }

            }


        }



    }

 
}


