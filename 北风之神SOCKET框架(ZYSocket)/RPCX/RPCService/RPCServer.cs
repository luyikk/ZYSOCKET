using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using ZYSocket.share;
using ZYSocket.Server;

namespace ZYSocket.RPCX.Service
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public delegate void BinaryInputOtherHandler(ReadBytes read, int cmd);
    public class RPCServer
    {
        public ZYSocketSuper Server { get; private set; }

        public RPCService Service { get; private set; }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int ReadOutTime
        {
            get; set;
        }

        public int MaxBuffsize { get; set; }

        /// <summary>
        /// 此IP是否可以连接?
        /// </summary>
        public event IsCanConnHandler IsCanConn;
        /// <summary>
        /// 其他数据包 不包含在RPC里面的
        /// </summary>
        public event BinaryInputOtherHandler BinaryInputOther;

        /// <summary>
        /// 是否使用TASK队列
        /// </summary>
        public bool IsUseTask { get; set; }

        /// <summary>
        /// 是否可以调用客户端同步等待函数
        /// </summary>
        public bool IsCallReturn { get; set; }


        /// <summary>
        /// 注册表
        /// </summary>
        private List<RPCCallObject> RegModule { get; set; }

        public RPCCallObject[] GetRegModule()
        {
            if (RegModule != null)
                return RegModule.ToArray();
            else
                return null;
        }

        public RPCServer()
        {
            Server = new ZYSocketSuper();
            MaxBuffsize = 1024 * 1024 * 2; //2M
            Init();
        }

        public RPCServer(string host, int port, int maxconnectcout, int maxbuffersize,int maxPackSize)
        {
            Server = new ZYSocketSuper(host, port, maxconnectcout, maxbuffersize);
            MaxBuffsize = maxPackSize;
            Init();
        }


        public void Start()
        {
            Server.Start();
            LogAction.Log("Server is Start");
        }

        public void Pause()
        {
            Server.Stop();
            LogAction.Log("Server is Pause");
        }

        private void Init()
        {
            RegModule = new List<RPCCallObject>();
            Service = new RPCService();        
            Server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputOffsetHandler);
            Server.Connetions = new ConnectionFilter(ConnectionFilter);
            Server.MessageInput = new MessageInputHandler(MessageInputHandler);
            Server.IsOffsetInput = true;
            ReadOutTime = 2000;
        }

        public void RegServiceModule(RPCCallObject o)
        {
            if (!RegModule.Contains(o))
            {
                RegModule.Add(o);
            }
        }

        private void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            if (socketAsync.UserToken != null)
            {
                Service.Disconnect(socketAsync.UserToken as RPCUserInfo);
            }

            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
            LogAction.Log(message);
        }

        /// <summary>
        /// 创建注册 RPGUSERINFO
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private RPCUserInfo NewRPCUserInfo(SocketAsyncEventArgs socketAsync)
        {
            RPCUserInfo tmp = new RPCUserInfo(socketAsync,MaxBuffsize,IsCallReturn);         
            tmp.OutTime = this.ReadOutTime;

            foreach (var item in RegModule)
            {
                tmp.RPC_Call.RegModule(item);
            }

            return tmp;
        }

        private bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {

            LogAction.Log(socketAsync.AcceptSocket.RemoteEndPoint.ToString() + " Connect");

            if (IsCanConn != null)
            {
                if (IsCanConn((IPEndPoint)socketAsync.AcceptSocket.RemoteEndPoint))
                {
                    socketAsync.UserToken = NewRPCUserInfo(socketAsync);
                    return true;
                }
                else
                    return false;
            }

            socketAsync.UserToken = NewRPCUserInfo(socketAsync);

            return true;
        }

        private void BinaryInputOffsetHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync)
        {
            try
            {
                RPCUserInfo userinfo = socketAsync.UserToken as RPCUserInfo;
                if (userinfo == null)
                {
                    Server.Disconnect(socketAsync.AcceptSocket);
                    return;
                }

                userinfo.Stream.Write(data, offset, count);

                byte[] datax;
                while (userinfo.Stream.Read(out datax))
                {
                    DataOn(datax, userinfo);
                }
            }
            catch (Exception er)
            {
                LogAction.Err(er.ToString());              
            }
        }

        private void DataOn(byte[] data, RPCUserInfo user)
        {
            ReadBytes read;
            int cmd;


            if (!Service.CallModule(data, user, IsUseTask, out read, out cmd))
            {
                if (BinaryInputOther != null)
                    BinaryInputOther(read, cmd);
            }

        }
    }
}
