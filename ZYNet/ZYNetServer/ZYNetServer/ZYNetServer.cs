using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZYSocket.Server;
using ZYSocket.share;
using System.Collections.Concurrent;
using ZYSocket.ZYNet.PACK;
using static ZYSocket.ZYNet.ServLOG;
namespace ZYSocket.ZYNet.Server
{

    /// <summary>
    /// 用于筛选此IP是否有权限连接
    /// </summary>
    /// <param name="address">IP地址</param>
    /// <returns>返回FALSE 拒绝联机,TRUE 可以连接</returns>
    public delegate bool UserConnectHandler(IPAddress address,int port);

    /// <summary>
    /// 用户断开处理
    /// </summary>
    /// <param name="session"></param>
    public delegate void UserDisconnectHandler(ZYNetSession session);

    /// <summary>
    /// 数据包输入
    /// </summary>
    /// <param name="session"></param>
    public delegate void UserDataInputHandler(ZYNetSession session,byte[] data);

    public class ZYNetServer
    {
        #region Only Static Object
        static object lockthis = new object();

        static ZYNetServer _My;

        public static ZYNetServer GetInstance()
        {
            lock (lockthis)
            {
                if (_My == null)
                    _My = new ZYNetServer();
            }

            return _My;
        }
        private ZYNetServer()
        {

          
        }
        #endregion


        /// <summary>
        /// 服务器Socket
        /// </summary>
        public ZYSocketSuper Service { get; private set; }

        /// <summary>
        /// 用于识别公网IP
        /// </summary>
        public ZYSocketSuper RegService { get; private set; }

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsRun { get; private set; }

        public int BufferMaxLength { get; private set; }

        public int RegPort { get; private set; }

        /// <summary>
        /// Session连接表
        /// </summary>
        public ConcurrentDictionary<long,ZYNetSession> SessionDiy { get; private set; }


        /// <summary>
        /// 用户连接 IP权限筛选
        /// </summary>
        public event UserConnectHandler UserConnectAuthority;

        /// <summary>
        /// 用户断开连接处理
        /// </summary>
        public event UserDisconnectHandler UserDisconnect;


        public event UserDataInputHandler UserDataInput;

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (IsRun)
            {
                LLOG("Service Is Start...",EventLogType.INFO);
                return;
            }

            BufferFormat.ObjFormatType = BuffFormatType.protobuf;
            ReadBytes.ObjFormatType = BuffFormatType.protobuf;

            ServicePackHander.GetInstance().Loading();

            SessionDiy = new ConcurrentDictionary<long, ZYNetSession>();

            Service = new ZYSocketSuper(RConfig.ReadString("Host"), RConfig.ReadInt("ServicePort"), RConfig.ReadInt("MaxConnectCount"),4096);//初始化
            Service.Connetions = new ConnectionFilter(Service_Connection); //注册连接回调         
            Service.BinaryOffsetInput = new BinaryInputOffsetHandler(Service_BinaryInput);//注册输入包输入回调
            Service.MessageInput = new MessageInputHandler(Service_UserDisconnect); //注册用户断开回调
            Service.IsOffsetInput = true;

            RegService = new ZYSocketSuper(RConfig.ReadString("Host"), RConfig.ReadInt("RegPort"), RConfig.ReadInt("MaxConnectCount"), 1024); //初始化注册端口
            RegService.Connetions = new ConnectionFilter(RegServer_Connection); //注册连接回调         
            RegService.BinaryInput =new BinaryInputHandler(RegServer_BinaryInput);//注册输入包输入回调
            RegService.MessageInput = new MessageInputHandler(RegServer_UserDisconnect); //注册用户断开回调

            Service.Start();
            RegService.Start();

            RegPort = RConfig.ReadInt("RegPort");
            BufferMaxLength = RConfig.ReadInt("BufferLength");


            LLOG("Service StartIng...", EventLogType.INFO);



            IsRun = true;
        }


        #region Service Codeing
        /// <summary>
        /// 有用户连接端口
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private bool Service_Connection(SocketAsyncEventArgs socketAsync)
        {
            bool IsConnect = true;

            var Ipaddress = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint);

            if (UserConnectAuthority != null)
                IsConnect= UserConnectAuthority(Ipaddress.Address, Ipaddress.Port);

            if(IsConnect)
            {
               
                ZYNetSession session = new ZYNetSession(MakeId(),socketAsync,new ZYNetRingBufferPool(BufferMaxLength));
                session.WANIP= Ipaddress.Address.ToString();
                socketAsync.UserToken = session;

                LLOG("Servv: " + Ipaddress.ToString()+" Connect", EventLogType.INFO);
            }



            return IsConnect;
        }

        /// <summary>
        /// 用户断开处理回调
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="socketAsync"></param>
        /// <param name="error"></param>
        private void Service_UserDisconnect(string msg, SocketAsyncEventArgs socketAsync,int error)
        {
            ZYNetSession session = socketAsync.UserToken as ZYNetSession;

            if (session != null)
            {
                if (UserDisconnect != null)
                    UserDisconnect(session);

                SessionDiy.TryRemove(session.Id, out session);

                RemoveSession tmp = new RemoveSession()
                {
                    Id = session.Id
                };

                SendAll(BufferFormat.FormatFCA(tmp));
            }

            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
            

            LLOG("Servv:" + msg, EventLogType.INFO);
        }


        /// <summary>
        /// 输入包输入回调 未处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="socketAsync"></param>
        private void Service_BinaryInput(byte[] data, int offset, int count,SocketAsyncEventArgs socketAsync)
        {
            try
            {
                ZYNetSession usertmp = (socketAsync.UserToken as ZYNetSession);

                if (usertmp != null)
                {
                    usertmp.BufferQueue.Write(data, offset, count);

                    byte[] datax;
                    while (usertmp.BufferQueue.Read(out datax))
                    {
                        DataOn(datax, usertmp);
                    }

                }


            }
            catch (Exception er)
            {
                LLOG(er.ToString(), EventLogType.ERR);
            }
        }

        /// <summary>
        /// 数据包输入 以处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="session"></param>
        private void DataOn(byte[] data,ZYNetSession session)
        {
            ReadBytes read = new ReadBytes(data);

            int length;
            int cmd;
            if(read.ReadInt32(out length)&&length==read.Length&&read.ReadInt32(out cmd))
            {
                if (cmd != -2000)
                {

                    if (!CmdToCallManager<ZYNetServer, ReadBytes, ZYNetSession>.GetInstance().pointerRun(this, cmd, read, session))
                    {
                        LLOG("Not Find CMD:" + cmd, EventLogType.ERR);
                    }
                }
                else
                {
                    ProxyData tmp;

                    if (read.ReadObject<ProxyData>(out tmp))
                    {
                        if (tmp.Source == session.Id)
                        {
                            if (tmp.Ids != null)
                            {
                                if (tmp.Ids.Contains(0))
                                {
                                    if (UserDataInput != null)
                                        UserDataInput(session, tmp.Data);
                                }

                                foreach (var Id in tmp.Ids)
                                {
                                    if (Id != 0 && SessionDiy.ContainsKey(Id))
                                    {
                                        Service.SendData(SessionDiy[Id].Asyn.AcceptSocket, data);
                                    }
                                }
                            }
                        }                 

                    }
                }
            }
        }

        /// <summary>
        /// 发送数据包给制定的客户端
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="data"></param>
        public void SendDataToClient(ZYNetSession Session, byte[] data)
        {
            ProxyData pdata = new ProxyData()
            {
                Data = data
            };

            byte[] buff = BufferFormat.FormatFCA(pdata);

            Send(Session, buff);
        }

        /// <summary>
        /// 发送数据包给所有客户端
        /// </summary>
        /// <param name="data"></param>
        public void SendDataToAllClient(byte[] data)
        {
            ProxyData pdata = new ProxyData()
            {
                Data = data
            };

            byte[] buff = BufferFormat.FormatFCA(pdata);

            SendAll(buff);
        }

        /// <summary>
        /// 发送数据包给所有客户端 内部专用
        /// </summary>
        /// <param name="data"></param>
        internal void SendAll(byte[] data)
        {
            

            foreach (var item in SessionDiy.Values)
            {
                Send(item, data);
            }

        }

        /// <summary>
        /// 发送数据包给制定的客户端 内部专用
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="data"></param>
        internal void Send(ZYNetSession Session,byte[] data)
        {
            Service.SendData(Session.Asyn.AcceptSocket, data);
        }


        #endregion






        #region RegServer Codeing
        /// <summary>
        /// 有用户连接端口
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private bool RegServer_Connection(SocketAsyncEventArgs socketAsync)
        {
            var Ipaddress = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint);
          

            LLOG("REGServ: " + socketAsync.AcceptSocket.RemoteEndPoint.ToString() + " Connect", EventLogType.INFO);

            return true;
        }

        /// <summary>
        /// 用户断开处理回调
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="socketAsync"></param>
        /// <param name="error"></param>
        private void RegServer_UserDisconnect(string msg, SocketAsyncEventArgs socketAsync, int error)
        {
            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();

            LLOG("REGServ:"+msg,EventLogType.INFO);
        }


        /// <summary>
        /// 输入包输入回调
        /// </summary>
        /// <param name="data"></param>
        /// <param name="socketAsync"></param>
        private void RegServer_BinaryInput(byte[] data,SocketAsyncEventArgs socketAsync)
        {
            try
            {
                if (socketAsync.AcceptSocket != null)
                {
                    ReadBytes read = new ReadBytes(data);

                    int length;
                    int Cmd;
                    long key;
                    int netport;
                    if (read.ReadInt32(out length) && length == read.Length && read.ReadInt32(out Cmd) && read.ReadInt64(out key) && read.ReadInt32(out netport))
                    {
                        if (Cmd == 100)
                        {
                            string ip = ((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint).Address.ToString(); //获取外网IP地址
                            int port =int.Parse(((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint).Port.ToString()); //获取端口号

                            if (SessionDiy.ContainsKey(key)) //检查是否包含此KEY
                            {
                                SessionDiy[key].WANIP = ip;
                                SessionDiy[key].WANPort = port;
                                SessionDiy[key].NatNextPort = netport - 1;

                                LLOG(string.Format("注册端口号: 客户端Id: {0} 外网IP地址: {1}:{3} 下次开放端口: {2}", key, ip, netport,port),EventLogType.INFO);

                                RegService.SendData(socketAsync.AcceptSocket, new byte[] { 1 });
                            }
                        }

                    }
                }
            }
            catch (Exception er)
            {
                LLOG(er.ToString(), EventLogType.ERR);
            }
        }
        #endregion

               

        static long _Next = 1000;
        public static long MakeId()
        {
            if (_Next >= 9223372036854775807)
                _Next=Interlocked.Exchange(ref _Next, 1000);

             _Next = Interlocked.Add(ref _Next, 1);

            return _Next;
        }
    }

 
}
