using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Xml.Linq;
using ZYSocket.Server;
using ZYSocket.share;
using P2PCMD;
namespace P2PSERVER
{
    class Program
    {
        public static ZYSocketSuper MainServer;
        public static ZYSocketSuper IpServer;
        /// <summary>
        /// 用户表
        /// </summary>
        public static ConcurrentDictionary<string, UserInfo> UserList = new ConcurrentDictionary<string, UserInfo>();

        static void Main(string[] args)
        {

          
            MainServer = new ZYSocketSuper(Config.Default.ServerIP, Config.Default.MServerPort, Config.Default.MaxConCount, 1024);
            MainServer.BinaryInput = new BinaryInputHandler(MainDataIn);
            MainServer.Connetions = new ConnectionFilter(MainConnetion);
            MainServer.MessageInput = new MessageInputHandler(MainExpection);

            IpServer = new ZYSocketSuper(Config.Default.ServerIP, Config.Default.RegServerPort, Config.Default.MaxConCount, 1024);
            IpServer.BinaryInput = new BinaryInputHandler(IpDataIn);
            IpServer.Connetions = new ConnectionFilter(IpConnetion);
            IpServer.MessageInput = new MessageInputHandler(IpExpection);

            IpServer.Start();
            MainServer.Start();


            Console.ReadLine();

        }

        static bool MainConnetion(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine("有连接到服务器:{0}", socketAsync.AcceptSocket.RemoteEndPoint);

            UserInfo usertmp = new UserInfo();
            usertmp.Asyn = socketAsync;
            socketAsync.UserToken = usertmp;
            usertmp.BufferQueue = new ZYNetRingBufferPoolV2();
            usertmp.WANIP = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint).Address.ToString();


            return true;
        }

        static void MainExpection(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            try
            {
                UserInfo tmp = (socketAsync.UserToken as UserInfo);

                if (tmp.Paw != null)
                {
                    UserList.TryRemove(tmp.Paw,out tmp);
                }

                socketAsync.UserToken = null;
                socketAsync.AcceptSocket.Close();

                Console.WriteLine(message);
            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }
        }


        static void MainDataIn(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            try
            {
                UserInfo usertmp = socketAsync.UserToken as UserInfo;

                if (usertmp != null)
                {
                    usertmp.BufferQueue.Write(data);

                    byte[] datax;
                    while (usertmp.BufferQueue.Read(out datax))
                    {
                        InputData(datax, usertmp);
                    }
                }
            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }
        }

        static void InputData(byte[] data, UserInfo user)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int length;
            int Cmd;
            if (read.ReadInt32(out length) && length == read.Length && read.ReadInt32(out Cmd))
            {
                PCMD pcmd = (PCMD)Cmd;


                switch (pcmd)
                {
                    case PCMD.REGION: //注册                      
                        string key1;
                        string lanhost;
                        string mac;

                        if (read.ReadString(out key1) && read.ReadString(out lanhost)&&read.ReadString(out mac))
                        {
                            user.Paw = key1;
                            user.LANhost = lanhost;
                            user.Mac = mac;

                            if (!UserList.ContainsKey(user.Paw))
                            {
                                if (UserList.TryAdd(user.Paw, user))
                                {
                                    Console.WriteLine(user.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "连接服务器 用户KEY:" + user.Paw + " 内网IP:" + user.LANhost + " 外网IP地址:" + user.WANIP);
                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.SET);
                                    MainServer.SendData(user.Asyn.AcceptSocket, tmp.Finish());
                                }
                            }
                        }
                        break;
                    case PCMD.GETALLMASK: //获取所有用户KEY列表
                        if (user.Paw != null)
                        {

                            BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.ALLUSER);


                            IEnumerable<UserInfo> userlist = UserList.Values.Where(p => p.Mac == user.Mac && p.Paw != user.Paw);

                            tmp.AddItem(userlist.Count());

                            foreach (var item in userlist)
                            {
                                tmp.AddItem(item.Paw);
                            }

                            MainServer.SendData(user.Asyn.AcceptSocket, tmp.Finish());
                        }
                        break;
                    case PCMD.CONN: //连接目标主机
                        string key;

                        if (read.ReadString(out key))//读取客户端KEY
                        {

                            if (UserList.ContainsKey(key) && !string.IsNullOrEmpty(user.WANhost) && !string.IsNullOrEmpty(user.CPort)) //检查此客户单是否是可以提供连接
                            {

                                UserInfo info = UserList[key]; //读取用户对象


                                if (info.Mac != user.Mac)
                                {
                                    return;
                                }

                                if (!user.WANIP.Equals(info.WANIP)) //如果不在同一个局域网
                                {

                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.NOWCONN);
                                    tmp.AddItem(user.WANhost + ":" + user.CPort);
                                    tmp.AddItem(user.Paw);
                                    MainServer.SendData(info.Asyn.AcceptSocket, tmp.Finish());                                   
                                }
                                else
                                { 
                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.NOWCONN);
                                    tmp.AddItem(user.LANhost + ":" + user.NatNetPort);
                                    tmp.AddItem(user.Paw);
                                    MainServer.SendData(info.Asyn.AcceptSocket, tmp.Finish());
                                }

                            }
                        }
                        break;
                    case PCMD.LEFTCONN:
                        string key2;
                        if (read.ReadString(out key2))
                        {
                            if (UserList.ContainsKey(key2) && !string.IsNullOrEmpty(user.WANhost) && !string.IsNullOrEmpty(user.CPort))
                            {
                                UserInfo info = UserList[key2];  //读取用户对象

                                if (info.Mac != user.Mac)
                                {
                                    return;
                                }

                                if (!user.WANIP.Equals(info.WANIP)) //如果不在同一个局域网
                                {
                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.LEFTCONN);
                                    tmp.AddItem(user.WANhost + ":" + user.CPort);
                                    tmp.AddItem(user.Paw);
                                    MainServer.SendData(info.Asyn.AcceptSocket, tmp.Finish());
                                }
                                else
                                {
                                   
                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.LEFTCONN);
                                    tmp.AddItem(user.LANhost + ":" + user.NatNetPort);
                                    tmp.AddItem(user.Paw);
                                    MainServer.SendData(info.Asyn.AcceptSocket, tmp.Finish());
                                }
                            }
                        }
                        break;
                    case PCMD.GETALLUSER:
                        {
                            BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.GETALLUSER);

                            IEnumerable<UserInfo> userlist = UserList.Values.Where(p => p.Mac == user.Mac&&p.Paw!=user.Paw);

                            tmp.AddItem(userlist.Count());

                            foreach (var item in userlist)
                            {
                                tmp.AddItem(item.Paw);
                            }

                            MainServer.SendData(user.Asyn.AcceptSocket, tmp.Finish());
                        }
                        break;
                    case PCMD.ProxyData:
                        {
                            string keys;
                            byte[] datas;

                            if (read.ReadString(out keys) && read.ReadByteArray(out datas))
                            {
                                if (UserList.ContainsKey(keys))
                                {
                                    BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.ProxyData);
                                    tmp.AddItem(user.Paw);
                                    tmp.AddItem(datas);
                                    MainServer.SendData(UserList[keys].Asyn.AcceptSocket, tmp.Finish());
                                }
                            }

                        }
                        break;
                }

            }
        }


        static bool IpConnetion(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine(socketAsync.AcceptSocket.RemoteEndPoint.ToString() + " 连接到注册更新端口");
            return true;
        }

        static void IpDataIn(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            if (socketAsync.AcceptSocket != null)
            {
                ReadBytes read = new ReadBytes(data);

                int length;
                int Cmd;
                string key;
                int netport;
                if (read.ReadInt32(out length) && length == read.Length && read.ReadInt32(out Cmd) && read.ReadString(out key) && read.ReadInt32(out netport))
                {
                    if (Cmd == 100)
                    {
                        string ip = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint).Address.ToString(); //获取外网IP地址
                        string port = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint).Port.ToString(); //获取端口号

                        if (UserList.ContainsKey(key)) //检查是否包含此KEY
                        {
                            UserList[key].WANhost = ip;
                            UserList[key].CPort = port;
                            UserList[key].NatNetPort = netport - 1;

                            Console.WriteLine("注册端口号: 客户端:Key {0} 外网IP地址: {1} 下次开放端口: {2}", key, ip, netport);
                        }
                    }

                }
            }
        }

        static void IpExpection(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            socketAsync.UserToken = null;
            Console.WriteLine(message);
        }


    }
}
