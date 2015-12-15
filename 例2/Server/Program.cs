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
                    if (cmd == 1000) //如果用户发送的是登入数据包
                    {
                        Login p;

                        if (read.ReadObject <Login>(out p))
                        {                           

                            if (p != null)
                            {
                                if (User.UserManger.GetUserDataManger().CheckUser(p.UserName, p.PassWord))//检查用户名密码是否正确
                                {
                                    User.UserInfo user = new User.UserInfo() //建立一个新的用户对象 并且初始化 用户名
                                    {
                                        UserName = p.UserName
                                    };

                                    socketAsync.UserToken = user; //设置USERTOKEN

                                    Message err = new Message() //初始化MESSAGE数据包类
                                    {
                                        Type = 2,
                                        MessageStr = "登入成功"
                                    };

                                    server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(err)); //发送此类

                                    Console.WriteLine(user.UserName + " 登入");

                                }
                                else
                                {
                                    Message err = new Message() //初始化用户名密码错误数据包
                                    {
                                        Type = 1,
                                        MessageStr = "用户名或密码错误"
                                    };

                                    server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(err));
                                }

                            }
                        }
                    }
                    else //现在还没登入 如果有其他命令的请求那么 断开连接
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
                   
                    user.BuffManger.Write(data);

                    byte[] pdata;
                    while (user.BuffManger.Read(out pdata))
                    {
                        DataOn(pdata, socketAsync);
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
                switch (cmd)
                {
                    case 800:
                        Ping pdata;
                        if (read.ReadObject<Ping>(out pdata)) //读取PING 数据包
                        {                          
                            if (pdata != null)
                            {
                                pdata.ServerReviceTime = DateTime.Now; //设置服务器时间
                                server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(pdata)); //发送返回
                            }
                        }
                        break;
                    case 1002:
                        ReadDataSet rd;

                        if (read.ReadObject<ReadDataSet>(out rd)) //读取请求DATASET 数据包
                        {
                           
                            if (rd != null)
                            {
                                rd.Data = new List<DataValue>();

                                rd.TableName = "table1";
                                rd.Data.Add(new DataValue()
                                {
                                    V1 = "第1个",
                                    V2 = "第2个",
                                    V3 = "第3个",
                                    V4 = "第4个",
                                    V5 = "第5个"
                                });

                                rd.Data.Add(new DataValue()
                                {
                                    V1 = "第6个",
                                    V2 = "第7个",
                                    V3 = "第8个",
                                    V4 = "第9个",
                                    V5 = "第10个"
                                });

                                rd.Data.Add(new DataValue()
                                {
                                    V1 = "第11个",
                                    V2 = "第12个",
                                    V3 = "第13个",
                                    V4 = "第14个",
                                    V5 = "第15个"
                                });


                                rd.Data.Add(new DataValue()
                                {
                                    V1 = "第16个",
                                    V2 = "第17个",
                                    V3 = "第18个",
                                    V4 = "第19个",
                                    V5 = "第20个"
                                });
                               
                                server.SendData(socketAsync.AcceptSocket, BufferFormat.FormatFCA(rd)); //发送

                                Console.WriteLine((socketAsync.UserToken as User.UserInfo).UserName + " 读取了" + rd.TableName);
                            }


                        }
                        break;
                }
            }
        }

    }
}
