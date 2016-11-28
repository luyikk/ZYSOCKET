using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using System.Net.Sockets;
using ZYSocket.share;

namespace AutoBufferServer
{
    class Program
    {
        //建立一个ZYSOCKETSERVER 对象 注意启动前应该先设置 App.Config 文件,
        //如果你不想设置App.Config文件 那么可以在构造方法里面传入相关的设置
        static ZYSocketSuper server = new ZYSocketSuper();

        static ZYAutoBuffer BufferRun = new ZYAutoBuffer(typeof(Program));

        static List<UserInfo> UserList = new List<UserInfo>();

        //程序入口
        static void Main(string[] args)
        {
            ReadBytes.ObjFormatType = BuffFormatType.protobuf;
            server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
            server.IsOffsetInput = true;
            server.Start(); //启动服务器

            Console.ReadLine();
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

            UserInfo user = socketAsync.UserToken as UserInfo;

            if (user != null)
            {
                UserList.Remove(user);
            }


            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
        }
        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());

            socketAsync.UserToken = null;
            return true;
        }



        /// <summary>
        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        static void BinaryInputHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync)
        {
            try
            {
                if (socketAsync.UserToken == null) //如果此SOCKET绑定的对象为NULL
                {
                    //注意这里为了 简单 所以就绑定了个 BuffList 类，本来这里应该绑定用户类对象，
                    //并在用户类里面建立 初始化 一个 BuffList 类，这样就能通过用户类保存更多的信息了。
                    //比如用户名，权限等等


                    socketAsync.UserToken = new ZYNetRingBufferPool(409600);
                }



                //BuffList 数据包组合类 如果不想丢数据就用这个类吧
                ZYNetRingBufferPool buff = socketAsync.UserToken as ZYNetRingBufferPool;


                if (buff != null)
                {

                    buff.Write(data, offset, count); //呵呵带offset的 可以省一次copy


                    byte[] pdata;
                    while (buff.Read(out pdata))
                    {
                        DataOn(pdata, socketAsync);
                    }
                }
                else
                {
                    UserInfo user = socketAsync.UserToken as UserInfo;

                    if(user!=null)
                    {
                        user.BuffPool.Write(data, offset, count); //呵呵带offset的 可以省一次copy


                        byte[] pdata;
                        while (user.BuffPool.Read(out pdata))
                        {
                            DataOn(pdata, user);
                        }

                    }

                }

            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }

        }

        static void DataOn(byte[] data, SocketAsyncEventArgs e)
        {

            //建立一个读取数据包的类 参数是数据包
            //这个类的功能很强大,可以读取数据包的数据,并可以把你发送过来的对象数据,转换对象引用

            ReadBytes read = new ReadBytes(data);

            int lengt; //数据包长度,用于验证数据包的完整性

            if(read.ReadInt32(out lengt)&&read.Length==lengt)
            {
                BufferRun.Run<SocketAsyncEventArgs>(read, e);
            }
        }

        static void DataOn(byte[] data, UserInfo user)
        {
            ReadBytes read = new ReadBytes(data);

            int lengt; //数据包长度,用于验证数据包的完整性

            if (read.ReadInt32(out lengt) && read.Length == lengt)
            {
                BufferRun.Run<UserInfo>(read, user);
            }
        }


        [CmdTypeOfAttibutes(1000)]
        public static void LogOn(SocketAsyncEventArgs e,string username,string password)
        {
            Console.WriteLine("UserName:{0} LogIn", username);

            //CHECK USER AND PASSWORD

            UserInfo user = new UserInfo()
            {
                UserName = username,
                PassWord = password,
                Asyn=e,
                BuffPool=e.UserToken as ZYNetRingBufferPool
            };

            e.UserToken = user;

            UserList.Add(user);

            server.Send(e.AcceptSocket,ZYAutoBuffer.Call(1000, true)); //TRUE =LOGON OK ;FALSE=LOGON Fail
        }

        [CmdTypeOfAttibutes(1001)]
        public static void Message(UserInfo user,string message)
        {
            Message tmp = new AutoBufferServer.Message();
            tmp.Date = DateTime.Now;
            tmp.Msg = message;
            tmp.UserName = user.UserName;

            foreach (var item in UserList)
            {
                if(item!=user)
                {
                    server.Send(item.Asyn.AcceptSocket, ZYAutoBuffer.Call(1001, tmp));
                }
            }
        }

    }
}
