using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using System.Net.Sockets;
using System.Net;
using ZYSocket.share;

namespace ZYSocket.RPC.Server
{
    public delegate bool IsCanConnHandler(IPEndPoint ipaddress);
    public delegate void BinaryInputOtherHandler(ReadBytes read, int cmd);

    public class RPCServer
    {
        public ZYSocketSuper Server { get; set; }

        public RPCService Service { get; set; }

        public List<object> RegModule { get; set; }



        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int ReadOutTime
        {
            get; set;
        }


        /// <summary>
        /// 此IP是否可以连接?
        /// </summary>
        public event IsCanConnHandler IsCanConn;
        /// <summary>
        /// 其他数据包 不包含在RPC里面的
        /// </summary>
        public event BinaryInputOtherHandler BinaryInputOther;

        public RPCServer()
        {
            Server = new ZYSocketSuper();
            Init();
        }

        public RPCServer(string host, int port, int maxconnectcout, int maxbuffersize)
        {
            Server = new ZYSocketSuper(host, port, maxconnectcout, maxbuffersize);
            Init();
        }


        public void Start()
        {
            Server.Start();
        }


        public void RegServiceModule(object o)
        {
           if(!RegModule.Contains(o))
           {
               RegModule.Add(o);
           }
        }




        private void Init()
        {
            RegModule = new List<object>();
            Service = new RPCService();
            Server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputOffsetHandler);
            Server.Connetions = new ConnectionFilter(ConnectionFilter);
            Server.MessageInput = new MessageInputHandler(MessageInputHandler);
            Server.IsOffsetInput = true;
           
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
        }

        /// <summary>
        /// 创建注册 RPGUSERINFO
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private RPCUserInfo NewRPCUserInfo(SocketAsyncEventArgs socketAsync)
        {
            RPCUserInfo tmp = new RPCUserInfo(socketAsync);
            tmp.RPC_Call.OutTime = ReadOutTime;
            foreach (var item in RegModule)
            {
                tmp.RPC_Call.RegModule(item);
            }

            return tmp;
        }

        private bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
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

        private void DataOn(byte[] data, RPCUserInfo user)
        {
            ReadBytes read;
            int cmd;
          

            if (Service.CallModule(data, user, out read, out cmd))
            {
              
            }
            else
            {
                if (BinaryInputOther != null)
                    BinaryInputOther(read, cmd);
            }
        }


    }
}
