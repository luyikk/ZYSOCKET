using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ZYSocket.share;
using ZYSocket.ClientB;
using System.Threading;

namespace P2PCLIENT
{

    public delegate void ClientConnToHandler(ConClient client);
    public delegate void ClientDataInHandler(string key,ConClient client, byte[] data);
    public delegate void ClientDisconHandler(ConClient client,string message);
    public delegate void ServerDisconHandler(string message);

    public delegate void GetAllUserListHandler(List<string> AllUserList);
    public class ClientInfo
    {

        public event ClientConnToHandler ClientConnToMe;

        public event ClientDataInHandler ClientDataIn;

        public event ClientDisconHandler ClientDiscon;

        public event ServerDisconHandler ServerDiscon;

        public event GetAllUserListHandler GetAllUserList;

        /// <summary>
        /// 服务器主连接SOCK
        /// </summary>
        public SocketClient Mainclient { get; private set; }

        /// <summary>
        /// 数据包缓冲池
        /// </summary>
        internal  ZYNetBufferReadStreamV2 Bufferlist { get; private set; }

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// 服务器注册端口号
        /// </summary>
        public int RegIpPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ConcurrentQueue<string> UserMaskList;


        private List<string> AllUser { get; set; }


        /// <summary>
        /// 客户端连接列表
        /// </summary>
        public ConcurrentDictionary<string, ConClient> ConnUserList { get; set; }


        public ConcurrentDictionary<string, ConClient> ProxyList { get; set; }

        /// <summary>
        /// 唯一标示KEY
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 当前绑定端口偏移量
        /// </summary>
        public int BindPort { get; set; }
        
        /// <summary>
        /// 尝试连接端口累计数量
        /// </summary>
        public int ResetConnt { get; set; }

        /// <summary>
        /// 区域区分码
        /// </summary>
        public string Mac { get; set; }


        


        public ClientInfo(string host, int port, int regipport,int bindminPort,int bindMaxPort,int resCount,string mac)
        {
            ResetConnt = resCount;
            ConnUserList = new ConcurrentDictionary<string, ConClient>(); //初始化客户端列表
            ProxyList = new ConcurrentDictionary<string, ConClient>();
            Key = Guid.NewGuid().ToString();//产生唯一标示KEY
            Bufferlist = new ZYNetBufferReadStreamV2();
            UserMaskList = new ConcurrentQueue<string>();
            BindPort = new Random().Next(bindminPort, bindMaxPort);
            this.Host = host;
            this.Port = port;
            this.RegIpPort = regipport;
            this.Mac = mac;

           
        }


        public void ConToServer()
        {
         
            Mainclient = new SocketClient();
            Mainclient.BinaryInput += new ClientBinaryInputHandler(DataIn);
            Mainclient.MessageInput += new ClientMessageInputHandler(Exption);

            if (Mainclient.Connect(Host, Port))
            {               
                Mainclient.StartRead();

                LogOut.LogIn("成功连接服务器", ActionType.ServerConn);

                string localip = ((IPEndPoint)(Mainclient.Sock.LocalEndPoint)).Address.ToString(); //获取本地局域网IP地址

                BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.REGION);
                tmp.AddItem(Key);
                tmp.AddItem(localip);
                tmp.AddItem(Mac);
                Mainclient.Send(tmp.Finish());
              
            }
            else
            {             
                LogOut.LogIn("不能连接到服务器", ActionType.ServerNotConn);

                if (ServerDiscon != null)
                    ServerDiscon("不能连接到服务器");
            }

        }

        /// <summary>
        /// 重新连接所有的用户
        /// </summary>
        public void ResetConnClient()
        {           
            BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.GETALLMASK);
            Mainclient.Send(tmp.Finish());
        }

        /// <summary>
        /// 释放所有用户
        /// </summary>
        public void DropConnUserList()
        {
            lock (ConnUserList)
            {
                foreach (KeyValuePair<string, ConClient> user in ConnUserList)
                {                   
                    user.Value.Sock.Close();
                }

                ConnUserList.Clear();
            }
        }

        /// <summary>
        /// 释放用户
        /// </summary>
        /// <param name="key"></param>
        public void DropConnUser(string key)
        {
            lock (ConnUserList)
            {
                if (ConnUserList.ContainsKey(key))
                {
                    ConnUserList[key].Sock.Close();
                    ConClient client;
                    ConnUserList.TryRemove(key, out client);
                }
               
            }
        }



        EventWaitHandle waitGetUser = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllUser()
        {
            GetAllUserList += ClientInfo_GetAllUserList;
            BeginGetAllUser();
            waitGetUser.Reset();
            waitGetUser.WaitOne();
            GetAllUserList -= ClientInfo_GetAllUserList;

            return AllUser;
        }
        

        void ClientInfo_GetAllUserList(List<string> AllUserList)
        {
            waitGetUser.Set();
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        public void BeginGetAllUser()
        {
            BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.GETALLUSER);
            Mainclient.Send(tmp.Finish());
        }


        /// <summary>
        /// 发送数据到指定的客户端
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SendData(string key, byte[] data)
        {
            if (ConnUserList.ContainsKey(key))
            {
                ConnUserList[key].SendData(data);
            }
            else
            {
                BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.ProxyData);
                tmp.AddItem(key);
                tmp.AddItem(data);
                Mainclient.Send(tmp.Finish());
            }
        }


        /// <summary>
        /// 数据包缓冲处理
        /// </summary>
        /// <param name="buffer"></param>
        private void DataIn(byte[] buffer)
        {
            
            Bufferlist.Write(buffer);

            byte[] datax;
            while (Bufferlist.Read(out datax))
            {
                BufferIn(datax);
            }

        }

        /// <summary>
        /// 数据包处理
        /// </summary>
        /// <param name="data"></param>
        private void BufferIn(byte[] data)
        {

            ReadBytesV2 read = new ReadBytesV2(data);
            int length;
            if (read.ReadInt32(out length) && length == read.Length)
            {
                int cmd;

                if (read.ReadInt32(out cmd))
                {
                    PCMD pcmd = (PCMD)cmd;


                    switch (pcmd)
                    {
                        case PCMD.SET: //准备就绪
                          
                            BufferFormatV2 tmp = new BufferFormatV2((int)PCMD.GETALLMASK);
                            Mainclient.Send(tmp.Finish());
                            break;
                        case PCMD.ALLUSER: //获取全部用户列表
                            try
                            {
                                int count;
                                if (read.ReadInt32(out count))
                                {
                                    for (int i = 0; i < count; i++)
                                    {
                                        string usermask;

                                        if (read.ReadString(out usermask))
                                        {
                                            UserMaskList.Enqueue(usermask);
                                        }
                                    }


                                    RunQueueList();
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                            }
                            break;
                        case PCMD.NOWCONN:  //立刻连接到指定IP端口
                            string host;
                            string key;

                            if (read.ReadString(out host) && read.ReadString(out key))
                            {
                                host = host + ":" + key;

                                SocketClient client = new SocketClient();
                            Tp:
                                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, BindPort++); //绑定端口
                                if (BindPort >= 60000)
                                    BindPort = 1000;

                                try
                                {
                                    client.Sock.Bind(endpoint); //如果无法绑定那么重新选个端口
                                }
                                catch
                                {
                                    goto Tp;
                                }

                                if (client.Connect(this.Host, RegIpPort)) //连接到注册端口
                                {
                                   
                                    BufferFormat tmpX = new BufferFormat(100);
                                    tmpX.AddItem(Key);
                                    tmpX.AddItem(BindPort);
                                    client.Send(tmpX.Finish());

                                    System.Threading.Thread.Sleep(50);

                                  
                                    BufferFormatV2 tmpX2 = new BufferFormatV2((int)PCMD.LEFTCONN);
                                    tmpX2.AddItem(key);
                                    Mainclient.Send(tmpX2.Finish());

                                    client.Close();

                                    System.Threading.Thread.Sleep(50);

                                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(RunConnToMe), host);

                                }
                            }
                            break;
                        case PCMD.LEFTCONN:
                            string host2;
                            string key2;
                            if (read.ReadString(out host2) && read.ReadString(out key2))
                            {
                                host2 = host2 + ":" + key2;
                                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(RunConnToMe), host2);
                            }
                            break;
                        case PCMD.GETALLUSER:
                            {
                                int count;

                                if (read.ReadInt32(out count))
                                {
                                    AllUser = new List<string>();

                                    for (int i = 0; i < count; i++)
                                    {
                                        string var;
                                        if (read.ReadString(out var))
                                        {
                                            AllUser.Add(var);
                                        }
                                        else
                                            break;
                                    }

                                    if (GetAllUserList != null)
                                        GetAllUserList(AllUser);

                                }

                            }
                            break;
                        case PCMD.ProxyData:
                            {
                                string keys;
                                byte[] buff;

                                if (read.ReadString(out keys) && read.ReadByteArray(out buff))
                                {
                                    if (ProxyList.ContainsKey(keys))
                                    {
                                        client_DataOutPut(keys, ProxyList[keys], buff);
                                    }
                                    else
                                    {
                                        ConClient client = new ConClient(keys);

                                        if (ProxyList.TryAdd(client.Key, client))
                                        {
                                            client_DataOutPut(keys, client, buff);
                                        }
                                    }
                                }

                            }
                            break;

                    }
                }
            }


        }





        void Exption(string message)
        {
            LogOut.LogIn("与服务器断开连接",ActionType.ServerDiscon);

            if (ServerDiscon != null)
                ServerDiscon("与服务器断开连接");
        }

        void RunQueueList()
        {
            try
            {
                if (UserMaskList.Count > 0) //如果列队数量大于0
                {
                Re:
                    string userkey;
                    
                    if (UserMaskList.TryDequeue(out userkey)) //挤出一个用户ID
                    {

                        if (userkey == Key)
                            goto Re;

                        SocketClient client = new SocketClient(); //建立一个 SOCKET客户端
                    Pt:
                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, BindPort++); //绑定当前端口

                        if (BindPort >= 60000)
                            BindPort = 1000;

                        try
                        {
                            client.Sock.Bind(endpoint); //如果无法绑定此端口 那么换个端口
                        }
                        catch
                        {
                            goto Pt;
                        }

                        if (client.Connect(Host, RegIpPort)) //连接注册服务器端口
                        {
                            BufferFormat tmp = new BufferFormat(100);
                            tmp.AddItem(Key);
                            tmp.AddItem(BindPort);
                            client.Send(tmp.Finish());

                            System.Threading.Thread.Sleep(50); //等待 50毫秒

                            BufferFormatV2 tmp2 = new BufferFormatV2((int)PCMD.CONN);
                            tmp2.AddItem(userkey);
                            Mainclient.Send(tmp2.Finish());
                            client.Close();//关闭客户端

                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogOut.LogIn(e.ToString(),ActionType.Error);
            }
        }

        /// <summary>
        /// 连接到指定的端口上
        /// </summary>
        /// <param name="o"></param>
        private void RunConnToMe(object o)
        {
            try
            {
                string[] iphost = o.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (iphost.Length == 3)
                {
                    int port;

                    if (int.TryParse(iphost[1], out port))
                    {
                        int maxport = port + ResetConnt; //最大端口等于提供的端口+尝试次数

                        for (int i = port + 1; i < maxport; i++) //循环连接的端口
                        {
                            ConClient client = new ConClient(iphost[0], i);

                            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, BindPort++);

                            if (BindPort >= 60000)
                                BindPort = 1000;

                            try
                            {
                                client.Sock.Sock.Bind(endpoint);
                                client.Key = iphost[2];
                                client.Conn += new ConnectionsHandlerFiler(client_Conn);
                                client.DataOutPut += new DataOutPutHandlerFiler(client_DataOutPut);
                                client.ExpOUtPut += new ExpOutPutHandlerFiler(client_ExpOUtPut);
                                client.ConnTo();
                                                             
                                LogOut.LogIn(string.Format("开始尝试本地端口{0} 连接到 {1}:{2}", BindPort - 1, iphost[0], i),ActionType.Message);
                            }
                            catch (SocketException e)
                            {
                             
                                LogOut.LogIn("错误无法绑定本地端口:" + e.Message, ActionType.Error);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogOut.LogIn("代码出错#1:"+ex.Message,ActionType.Error);
            }
            finally
            {
                RunQueueList();
            }

        }

      


        void client_Conn(ConClient conClient, bool conn)
        {
            if (conClient != null && conn)
            {
               
                LogOut.LogIn(string.Format("连接到:{0}  ---->OK", conClient.Sock.Sock.RemoteEndPoint.ToString()),ActionType.Message);

                if (!ConnUserList.ContainsKey(conClient.Key))
                {
                    if (ConnUserList.TryAdd(conClient.Key, conClient))
                    {
                        conClient.Sock.StartRead();

                        if (ClientConnToMe != null)
                            ClientConnToMe(conClient);
                    }
                }
                else
                {
                    conClient.Sock.Close();
                }
                
            }
            else
            {                
                LogOut.LogIn(string.Format("无法连接到指定地址端口 {0}:{1}", conClient.Host, conClient.Port), ActionType.Message);
                conClient.Sock.Close();
            }
        }

        void client_DataOutPut(string key,ConClient conClient, byte[] Data)
        {
            if (ClientDataIn != null)
                ClientDataIn(key,conClient, Data);
          
        }

        void client_ExpOUtPut(ConClient conClient, string Message)
        {
            if(ConnUserList.ContainsKey(conClient.Key))
                ConnUserList.TryRemove(conClient.Key, out conClient);

            LogOut.LogIn(string.Format("客户连接断开 {0}:{1}", conClient.Host+":"+conClient.Port, conClient.Key),ActionType.Message);

            if (ClientDiscon != null)
                ClientDiscon(conClient, string.Format("客户连接断开 {0}:{1}", conClient.Host + ":" + conClient.Port, conClient.Key));


        }
    }
}
