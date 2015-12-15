using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ZYSocket.Server;
using ZYSocket.RPC;
using ZYSocket.RPC.Server;
using ZYSocket.share;

namespace RPCTest
{
    class Program
    {
        //建立一个ZYSOCKETSERVER 对象 注意启动前应该先设置 App.Config 文件,
        //如果你不想设置App.Config文件 那么可以在构造方法里面传入相关的设置
        static ZYSocketSuper server = new ZYSocketSuper();

        static RPCService service = new RPCService();

        //程序入口
        static void Main(string[] args)
        {

            service.RegModule(new TestUserLogOn()); //添加ITest
            service.RegModule(new ITest());
            service.RegModule(new TestTow());



            server.BinaryInput = new BinaryInputHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
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
            try
            {
                Console.WriteLine(message);
                socketAsync.UserToken = null;
                socketAsync.AcceptSocket.Close();
                socketAsync.AcceptSocket.Dispose();

            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }
        }
        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            try
            {
                Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());
                socketAsync.UserToken = null;
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        static void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync)
        {

            try
            {
                if (socketAsync.UserToken == null) //如果此SOCKET绑定的对象为NULL
                {
                    //注意这里为了 简单 所以就绑定了个 ZYNetBufferReadStreamV2 类，本来这里应该绑定用户类对象，
                    //并在用户类里面建立 初始化 一个 ZYNetBufferReadStreamV2 类，这样就能通过用户类保存更多的信息了。
                    //比如用户名，权限等等
                    socketAsync.UserToken = new ZYNetRingBufferPoolV2(1024 * 1024 * 30);
                }


                ZYNetRingBufferPoolV2 stream = socketAsync.UserToken as ZYNetRingBufferPoolV2; //最新的数据包整合类

                stream.Write(data);

                byte[] datax;
                while (stream.Read(out datax))
                {
                    DataOn(datax, socketAsync);
                }
            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }

        }

        static void DataOn(byte[] data, SocketAsyncEventArgs e)
        {
            ZYClient_Result_Return returnValue;
            if (service.CallModule(data, e, out returnValue))
            {
                if (returnValue != null)
                    server.SendData(e.AcceptSocket, BufferFormatV2.FormatFCA(returnValue));
            }
            else
            {
                // 其他类型数据包

            }

        }
    }
}
