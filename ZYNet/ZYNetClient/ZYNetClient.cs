using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ZYSocket.ZYNet.PACK;
using System.Collections.Concurrent;
using System.Net.Sockets;
using static ZYSocket.ZYNet.Client.LogOut;

namespace ZYSocket.ZYNet.Client
{
    public delegate void ClientConnToHandler(long Id);
    public delegate void ClientDataInHandler(long Id, byte[] data);
    public delegate void ClientDisconHandler(long Id, string message);
    public delegate void ServerDisconHandler(string message);

    public class ZYNetClient
    {
        internal SocketClient MainClient { get; private set; }

        internal ZYNetRingBufferPool BufferQueue { get; private set; }

        object lockObj = new object();

        /// <summary>
        /// 客户端连接列表
        /// </summary>
        public ConcurrentDictionary<long, SessionObj> ConnUserList { get; private set; }

        public  ConcurrentQueue<long> UserMaskList { get; private set; }

        /// <summary>
        /// 唯一标示Id
        /// </summary>
        public long Id { get; private set; }

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
        public int Group { get; set; }

        /// <summary>
        /// 服务器注册端口
        /// </summary>
        public string RegHost { get; private set; }

        /// <summary>
        /// 服务器注册端口号
        /// </summary>
        public int RegPort { get; private set; }

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public readonly int BufferPoolLength;

        /// <summary>
        /// 和其他客户端连接成功回调
        /// </summary>
        public event ClientConnToHandler ConnectToMe;

        /// <summary>
        /// 数据包输入
        /// </summary>
        public event ClientDataInHandler DataInput;

        /// <summary>
        /// 其他客户端和你断开连接
        /// </summary>
        public event ClientDisconHandler SessionDisconnect;

        /// <summary>
        /// 和服务器断开连接
        /// </summary>
        public event ServerDisconHandler ServerDisconnect;

        public ZYNetClient()
        {
            BufferPoolLength = 1024 * 1024 * 2;
            BufferFormat.ObjFormatType = BuffFormatType.protobuf;
            ReadBytes.ObjFormatType = BuffFormatType.protobuf;
            ConnUserList = new ConcurrentDictionary<long, SessionObj>();
            UserMaskList = new ConcurrentQueue<long>();
            BufferQueue = new ZYNetRingBufferPool(BufferPoolLength);            
            BindPort  = new Random().Next(1000, 60000);
            ResetConnt =1;
            Group = 0;
        }

        public ZYNetClient(ushort minport,ushort maxport,int bufferPoolLength)
        {
            BufferPoolLength = bufferPoolLength;
            BufferFormat.ObjFormatType = BuffFormatType.protobuf;
            ReadBytes.ObjFormatType = BuffFormatType.protobuf;
            ConnUserList = new ConcurrentDictionary<long, SessionObj>();
            UserMaskList = new ConcurrentQueue<long>();
            BufferQueue = new ZYNetRingBufferPool(BufferPoolLength);
            BindPort = new Random().Next(minport, maxport);
            ResetConnt = 1;
            Group = 0;
        }


        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public bool Connect(string host,int port)
        {
            if (MainClient != null)
                throw new ObjectDisposedException("MainClient", "连接已经初始化,无法再连接,请重新 new ZYNetClient");

            MainClient = new SocketClient();
            MainClient.BinaryInput += Client_BinaryInput;
            MainClient.MessageInput += Client_MessageInput;

            if (MainClient.Connect(host, port))
            {
                MainClient.StartRead();

                LLOG("成功连接服务器", ActionType.ServerConn);
                RegHost = host;

                string localip = ((IPEndPoint)(MainClient.Sock.LocalEndPoint)).Address.ToString(); //获取本地局域网IP地址

                RegSession session = new RegSession()
                {
                    LocalHost=localip,
                    Group=Group                   
                };

                SendToServer(BufferFormat.FormatFCA(session));

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 获取所有的用户
        /// </summary>
        public void GetAllUserSession()
        {

            GetAllSession tmp = new GetAllSession()
            {
                UserIds=new List<long>(),
                IsSuccess=false
            };

            SendToServer(BufferFormat.FormatFCA(tmp));
        }
        


        /// <summary>
        /// 释放所有用户
        /// </summary>
        public void DropConnUserList()
        {
            lock (ConnUserList)
            {
                foreach (var user in ConnUserList)
                {
                    if(user.Value.IsConnect&&user.Value.Client!=null)
                        user.Value.Client.Sock.Close();                   
                }

                ConnUserList.Clear();
            }
        }

        /// <summary>
        /// 释放用户
        /// </summary>
        /// <param name="key"></param>
        private void DropConnUser(long key)
        {
            lock (ConnUserList)
            {
                if (ConnUserList.ContainsKey(key))
                {
                    var session = ConnUserList[key];

                    if(session.IsConnect&&session.Client!=null)
                        session.Client.Sock.Close();

                    session.IsConnect = false;

                    ConnUserList.TryRemove(key, out session);
                }

            }
        }

    
        /// <summary>
        /// 与服务器断开连接
        /// </summary>
        /// <param name="message"></param>
        private void Client_MessageInput(string message)
        {
            if (ServerDisconnect != null)
                ServerDisconnect(message);

              LLOG("与服务器断开连接", ActionType.ServerDiscon);
        }

        private void Client_BinaryInput(byte[] buffer)
        {
            BufferQueue.Write(buffer);

            byte[] datax;
            while (BufferQueue.Read(out datax))
            {
                BufferIn(datax);
            }
        }

        /// <summary>
        /// 数据包处理
        /// </summary>
        /// <param name="data"></param>
        private  void BufferIn(byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int length;
            int cmd;
            if (read.ReadInt32(out length) && length == read.Length&&read.ReadInt32(out cmd))
            {
                switch(cmd)
                {
                    case -1000:
                        {
                            RegSession session;

                            if(read.ReadObject<RegSession>(out session))
                            {
                                if(session.IsSuccess)
                                {
                                    this.Id = session.Id;
                                    this.RegPort = session.Port;
                                    LLOG("MY ID:" + this.Id, ActionType.Message);
                                    GetAllUserSession();
                                }
                                else
                                {
                                    LLOG(session.Msg, ActionType.Error);
                                    MainClient.Close();

                                }
                            }

                        }
                        break;
                    case -1001:
                        {
                            GetAllSession allsession;

                            if (read.ReadObject<GetAllSession>(out allsession))
                            {
                                if (allsession.IsSuccess&&allsession.UserIds!=null)
                                {
                                    CheckDroupConnect(allsession);

                                    foreach (var Id in allsession.UserIds)
                                    {
                                        if(!ConnUserList.ContainsKey(Id))
                                        {
                                            SessionObj tmp = new SessionObj()
                                            {
                                                Id=Id
                                            };

                                            ConnUserList.AddOrUpdate(Id, tmp, (a,b) => tmp);

                                            UserMaskList.Enqueue(Id);

                                        }
                                    }

                                    RunQueueList();
                                }                        
                            }
                        }
                        break;
                    case -1002:
                        {
                            ConnectTo connto;

                            if(read.ReadObject<ConnectTo>(out connto))
                            {
                                if (connto.IsSuccess)
                                    RegConnectTo(connto.Host, connto.Port, connto.Id);
                            }
                        }
                        break;
                    case -1003:
                        {
                            LEFTConnect leftconnto;

                            if (read.ReadObject<LEFTConnect>(out leftconnto))
                            {
                                if(leftconnto.IsSuccess)
                                    RunConnToMe(leftconnto.Host, leftconnto.Port, leftconnto.Id);
                            }
                        }
                        break;
                    case -1004:
                        {
                            AddSession addsession;

                            if (read.ReadObject<AddSession>(out addsession))
                            {
                                if (addsession.Id != this.Id)
                                {
                                    if (!ConnUserList.ContainsKey(addsession.Id))
                                    {
                                        SessionObj tmp = new SessionObj()
                                        {
                                            Id = addsession.Id
                                        };

                                        ConnUserList.AddOrUpdate(addsession.Id, tmp, (a, b) => tmp);

                                        LLOG("Add Session:" + addsession.Id, ActionType.Message);
                                    }
                                }
                            }
                        }
                        break;
                    case -1005:
                        {
                            RemoveSession remove;
                            if(read.ReadObject<RemoveSession>(out remove))
                            {
                                if(ConnUserList.ContainsKey(remove.Id))
                                {
                                    DropConnUser(remove.Id);

                                    LLOG("Remove Session:" + remove.Id, ActionType.Message);
                                }
                            }

                        }
                        break;
                    case -2000:
                        {
                            ProxyData proxydata;

                            if(read.ReadObject<ProxyData>(out proxydata))
                            {
                                if (DataInput != null)
                                    DataInput(proxydata.Source, proxydata.Data);
                            }
                        }
                        break;
                }
            }
        }


        void RegConnectTo(string host,int port, long id)
        {
            SocketClient client = new SocketClient();

            int ReCount = 10;

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
                ReCount--;
                if (ReCount > 0)
                    goto Tp;
                else
                    return;
            }

            if (client.Connect(this.RegHost, this.RegPort)) //连接到注册端口
            {

                client.StartRead();
                client.BinaryInput += (data) =>
                  {
                      if (data[0] == 1)
                      {
                          BufferFormat tmp2 = new BufferFormat(-1003);
                          tmp2.AddItem(id);
                          MainClient.Send(tmp2.Finish());
                          client.Close();
                          RunConnToMe(host, port, id);
                      }
                  };

                BufferFormat tmpX = new BufferFormat(100);
                tmpX.AddItem(this.Id);
                tmpX.AddItem(BindPort);
                client.Send(tmpX.Finish());

            }
            else  //如果无法绑定此端口 那么换个端口
            {
                if (client.socketError == SocketError.AddressAlreadyInUse)
                {
                   
                    ReCount--;
                    if (ReCount > 0)
                    {
                        client = new SocketClient();
                        goto Tp;
                    }
                    else
                        return;
                }
            }
        }



        /// <summary>
        /// 连接到指定的端口上
        /// </summary>
        /// <param name="o"></param>
        private async void RunConnToMe(string host, int port, long id)
        {
           await Task.Run(() =>
            {
                try
                {

                    int maxport = port +1+ResetConnt; //最大端口等于提供的端口+尝试次数

                    for (int i = port + 1; i < maxport; i++) //循环连接的端口
                    {
                        ConClient client = new ConClient(host, i,BufferPoolLength);

                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, BindPort++);

                        if (BindPort >= 60000)
                            BindPort = 1000;

                        try
                        {
                            client.Sock.Sock.Bind(endpoint);
                            client.Key = id;
                            client.Conn += new ConnectionsHandlerFiler(client_Conn);                          
                            client.ConnTo();

                            LLOG(string.Format("开始尝试本地端口{0} 连接到 {1}:{2}", BindPort - 1, host, i), ActionType.None);
                        }
                        catch (SocketException e)
                        {
                            LLOG("错误无法绑定本地端口:" + e.Message, ActionType.Error);
                        }

                    }


                }
                catch (Exception ex)
                {
                    LLOG("代码出错#1:" + ex.Message, ActionType.Error);
                }
                finally
                {
                    RunQueueList();
                }
            });
        }



        void RunQueueList()
        {
            try
            {
                if (UserMaskList.Count > 0) //如果列队数量大于0
                {



                    Re:
                    long userkey;

                    if (UserMaskList.TryDequeue(out userkey)) //挤出一个用户ID
                    {

                        if (userkey == this.Id)
                            goto Re;

                        SocketClient client = new SocketClient(); //建立一个 SOCKET客户端

                        int Re = 10;

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
                            Re--;

                            if (Re > 0)
                                goto Pt;
                            else
                                return;
                        }

                        if (client.Connect(RegHost, RegPort)) //连接注册服务器端口
                        {
                            client.StartRead();

                            client.BinaryInput += (data) =>
                            {
                                if (data[0] == 1)
                                {

                                    BufferFormat tmp2 = new BufferFormat(-1002);
                                    tmp2.AddItem(userkey);
                                    MainClient.Send(tmp2.Finish());
                                    client.Close();//关闭客户端
                                }
                            };

                            BufferFormat tmp = new BufferFormat(100);
                            tmp.AddItem(Id);
                            tmp.AddItem(BindPort);
                            client.Send(tmp.Finish());                           

                        }
                        else//如果无法绑定此端口 那么换个端口
                        {
                            if (client.socketError == SocketError.AddressAlreadyInUse)
                            {
                                client = new SocketClient();

                                Re--;

                                if (Re > 0)
                                {                                    
                                    client = new SocketClient();
                                    goto Pt;
                                }
                                else
                                    return;

                                
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LLOG(e.ToString(), ActionType.Error);
            }
        }

    

        /// <summary>
        ///  检查失效的连接
        /// </summary>
        /// <param name="allsession"></param>
        private void CheckDroupConnect(GetAllSession allsession)
        {

            var tmpRemoveList = new List<long>();

            foreach (var key in ConnUserList.Keys)
            {
                if (!allsession.UserIds.Contains(key))
                {
                    tmpRemoveList.Add(key);
                }
            }

            foreach (var removekey in tmpRemoveList)
            {
                DropConnUser(removekey);
            }

        }

        private void SendToServer(byte[] data)
        {
            MainClient.Send(data);
        }


        void client_Conn(ConClient conClient, bool conn)
        {
            if (conClient != null && conn)
            {

                LLOG(string.Format("连接到:{0}  ---->OK", conClient.Sock.Sock.RemoteEndPoint.ToString()), ActionType.None);

                if (ConnUserList.ContainsKey(conClient.Key))
                {

                    lock (lockObj)
                    {
                        if (!ConnUserList[conClient.Key].IsConnect)
                        {
                            conClient.DataOutPut += new DataOutPutHandlerFiler(client_DataOutPut);
                            conClient.ExpOUtPut += new ExpOutPutHandlerFiler(client_ExpOUtPut);

                            var session = ConnUserList[conClient.Key];
                            session.IsConnect = true;
                            session.Client = conClient;

                            conClient.Sock.StartRead();


                            if (ConnectToMe != null)
                                ConnectToMe(conClient.Key);
                        }
                        else
                        {
                            conClient.Sock.Close();
                        }
                    }

                }
                else
                {
                    conClient.Sock.Close();
                }

            }
            else
            {
                LLOG(string.Format("无法连接到指定地址端口 {0}:{1}", conClient.Host, conClient.Port), ActionType.None);
                conClient.Sock.Close();
            }
        }

        void client_DataOutPut(long key, ConClient conClient, byte[] Data)
        {                                    
            if (DataInput != null)
                DataInput(key, Data);
        }


        void client_ExpOUtPut(ConClient conClient, string Message)
        {

            SessionObj session = null;
            if (ConnUserList.ContainsKey(conClient.Key))
            {
                session = ConnUserList[conClient.Key];
                session.IsConnect = false;

            }

            LLOG(string.Format("客户连接断开 {0}:{1}", conClient.Host + ":" + conClient.Port, conClient.Key), ActionType.None);

            if (SessionDisconnect != null)
                SessionDisconnect(conClient.Key,string.Format("客户连接断开 {0}:{1}", conClient.Host + ":" + conClient.Port, conClient.Key));


        }


        public List<long> GetNotConnectSession()
        {
            var ids = from p in ConnUserList.Values.Where(p =>(p.IsConnect == false||p.Client==null)&&p.Id!=this.Id)
                      select p.Id;
            return ids.ToList();
        }

        public List<SessionObj> GetConnectSession()
        {
            return ConnUserList.Values.Where(p => p.IsConnect == true&&p.Id!=this.Id).ToList();              
        }

        /// <summary>
        /// 发送数据包给所有人,包括服务器
        /// </summary>
        /// <param name="data"></param>
        public void SendDataToALL(byte[] data)
        {
            var list= GetNotConnectSession();
            list.Add(0);

            ProxyData tmp = new ProxyData();
            tmp.Source = this.Id;
            tmp.Data = data;
            tmp.Ids = list;
            MainClient.BeginSendData(BufferFormat.FormatFCA(tmp));

            foreach (var item in GetConnectSession())
            {
                item.Client.SendData(data);
            }
            
        }

        /// <summary>
        /// 发送数据包给所有人,不包括服务器
        /// </summary>
        /// <param name="data"></param>
        public void SendDataToALLClient(byte[] data)
        {
            var list = GetNotConnectSession();
            if (list.Count > 0)
            {
                ProxyData tmp = new ProxyData();
                tmp.Source = this.Id;
                tmp.Data = data;
                tmp.Ids = list;
                MainClient.BeginSendData(BufferFormat.FormatFCA(tmp));
            }

            foreach (var item in GetConnectSession())
            {
                item.Client.SendData(data);
            }

        }


        public void SendData(long Id,byte[] data)
        {
            if(ConnUserList.ContainsKey(Id))
            {
                var session = ConnUserList[Id];

                if (session.IsConnect && session.Client != null)
                    session.Client.SendData(data);
                else
                {
                    ProxyData tmp = new ProxyData();
                    tmp.Source = this.Id;
                    tmp.Data = data;
                    tmp.Ids = new List<long>();
                    tmp.Ids.Add(Id);
                    MainClient.BeginSendData(BufferFormat.FormatFCA(tmp));
                }

            }
        }

        public void SendDataToServer(byte[] data)
        {
            ProxyData tmp = new ProxyData();
            tmp.Source = this.Id;
            tmp.Data = data;
            tmp.Ids = new List<long>();
            tmp.Ids.Add(0);
            MainClient.BeginSendData(BufferFormat.FormatFCA(tmp));
        }


    }
}
