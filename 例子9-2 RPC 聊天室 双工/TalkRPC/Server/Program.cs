using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using ZYSocket.share;
using ZYSocket.RPC;
using ZYSocket.RPC.Server;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static ZYSocketSuper server = new ZYSocketSuper();
        static RPCService service = new RPCService();

        static void Main(string[] args)
        {
            service.RegModule(new TalkService()); //添加ITest
        

            server.BinaryInput = new BinaryInputHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
            server.Start(); //启动服务器

            while (true)
                Console.ReadLine();
        }

        static void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            try
            {
                Console.WriteLine(message);

                service.Disconnect(socketAsync); //注意释放下 RPC

                socketAsync.UserToken = null;
                socketAsync.AcceptSocket.Close();
                socketAsync.AcceptSocket.Dispose();

            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }
        }

        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            try
            {
                Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());
                socketAsync.UserToken = new UserInfo(socketAsync);
                return true;
            }
            catch
            {
                return false;
            }

        }

        static void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync)
        {

            try
            {               

                UserInfo user = socketAsync.UserToken as UserInfo; //最新的数据包整合类

                user.Stream.Write(data);

                byte[] datax;
                while (user.Stream.Read(out datax))
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
