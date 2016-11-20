using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using ZYSocket.share;
using System.Net.Sockets;
using BuffLibrary;

namespace Server
{
    class Program
    {
        static ZYSocketSuper server = new ZYSocketSuper();

        static void Main(string[] args)
        {
            PackerHander.GetInstance().Loading();

            server.BinaryInput = new BinaryInputHandler(BinaryInputHandler);
            server.Connetions = new ConnectionFilter(ConnectionFilter);
            server.MessageInput = new MessageInputHandler(MessageInputHandler);
            server.MessageOut += new EventHandler<LogOutEventArgs>(server_MessageOut);
            server.Start();

            Console.ReadLine();
        }

        static void server_MessageOut(object sender, LogOutEventArgs e)
        {
                //输出消息
                Console.WriteLine(e.Mess);
        }


         /// <summary>
        /// 用户断开代理（你可以根据socketAsync 读取到断开的
        /// </summary>
        /// <param name="message">断开消息</param>
        /// <param name="socketAsync">断开的SOCKET</param>
        /// <param name="erorr">错误的ID</param>
        static void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            if (socketAsync.UserToken != null) 
            {
                User.UserInfo user = socketAsync.UserToken as User.UserInfo;

                Console.WriteLine(user.UserName + " 退了");

                socketAsync.UserToken = null; //这里一定要设置为NULL 否则出现的错误 很爽
            }
        }
        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString());          
          
            return true;
        }

        /// <summary>
        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        static void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            if (socketAsync.UserToken == null) //如果用户第一次登入
            {
                ReadBytes read = new ReadBytes(data); 

                int length;
                int cmd;

                if (read.ReadInt32(out length) && length == read.Length && read.ReadInt32(out cmd))
                {
                    if (!CmdToCallManager<ZYSocketSuper,int,ReadBytes,SocketAsyncEventArgs>.GetInstance().pointerRun(server,cmd,read,socketAsync)) //如果用户发送的是登入数据包
                    {
                        server.Disconnect(socketAsync.AcceptSocket);
                    }

                }
                else //无法读取数据包 断开连接
                {
                    server.Disconnect(socketAsync.AcceptSocket);
                }

            }
            else
            {
                User.UserInfo user = socketAsync.UserToken as User.UserInfo; //读取用户USERTRKEN

                if (user != null)
                {
                   
                    if (user.BuffManger.Write(data)) //这里的 4表示 数据包长度是用4字节的整数存储的 Int
                    {
                        byte[] pdata;

                        while (user.BuffManger.Read(out pdata))
                        {
                            DataOn(pdata, socketAsync);
                        }
                    }

                }

            }

        }

        static void DataOn(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            ReadBytes read = new ReadBytes(data);

            int length;
            int cmd;

            if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && length == read.Length)
            {
                if (!CmdToCallManager<ZYSocketSuper, ReadBytes, SocketAsyncEventArgs>.GetInstance().pointerRun(server, cmd, read, socketAsync)) //如果用户发送的是登入数据包
                {
                    server.Disconnect(socketAsync.AcceptSocket);
                }
            }
        }

    }
}
