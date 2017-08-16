using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using System.Net.Sockets;
using ZYSocket.share;
using ZYSocket.AsyncSend;
namespace TestServer
{
    class Program
    {
        //建立一个ZYSOCKETSERVER 对象 注意启动前应该先设置 App.Config 文件,
        //如果你不想设置App.Config文件 那么可以在构造方法里面传入相关的设置
        static  ZYSocketSuper server = new ZYSocketSuper(); 
              

        //程序入口
        static void Main(string[] args)
        {
            System.Threading.ExecutionContext.SuppressFlow();
            server.BinaryInput = new BinaryInputHandler(BinaryInputHandler); //设置输入代理
            server.Connetions=new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput=new MessageInputHandler(MessageInputHandler); //设置 客户端断开
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
            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
        }
        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());
            socketAsync.UserToken = new AsyncSend(socketAsync.AcceptSocket);

            
            return true;
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
                server.Send(socketAsync.UserToken as AsyncSend, data);

                // server.SendData(socketAsync.AcceptSocket, data);
                 // server.Send(socketAsync.AcceptSocket, data);

                // socketAsync.AcceptSocket.Send(data);

                //AsyncSend tmp = socketAsync.UserToken as AsyncSend;

                //if (tmp != null)
                //{
                //    tmp.Send(data);
                //}

            }
            catch
            {

            }
        }
        
    }
}
