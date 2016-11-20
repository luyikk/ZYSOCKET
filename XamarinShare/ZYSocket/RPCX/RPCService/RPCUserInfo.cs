using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks.Schedulers;
using ZYSocket.share;

namespace ZYSocket.RPCX.Service
{
    public class RPCUserInfo
    {
        public QueuedTaskScheduler Scheduler { get; set; }

        public SocketAsyncEventArgs Asyn { get; private set; }
        public ZYNetRingBufferPool Stream { get; private set; }
        public object UserToken { get; set; }
        public RPC RPC_Call { get; private set; }


        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="data"></param>
        public virtual void SendData(byte[] data)
        {
            try
            {
                Asyn.AcceptSocket.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, Asyn.AcceptSocket);

            }
            catch (Exception)
            {

            }
        }
        void AsynCallBack(IAsyncResult result)
        {
            try
            {
                Socket sock = result.AsyncState as Socket;

                if (sock != null)
                {
                    sock.EndSend(result);
                }
            }
            catch (Exception)
            {

            }
        }
                
        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int OutTime { get { return RPC_Call.OutTime; } set { RPC_Call.OutTime = value; } }


     
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (Asyn != null && Asyn.AcceptSocket != null)
                Asyn.AcceptSocket.Disconnect(false);
        }

        /// <summary>
        /// 获取接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRPC<T>()
        {
            return RPC_Call.GetRPC<T>();
        }

        public RPCUserInfo(SocketAsyncEventArgs asyn, int maxSize,bool isCallReturn)
        {
            Scheduler = new QueuedTaskScheduler(4);
            RPC_Call = new RPC(isCallReturn);
            RPC_Call.CallBufferOutSend += SendData;
            Stream = new ZYNetRingBufferPool(maxSize);
            this.Asyn = asyn;
        }
    }
}
