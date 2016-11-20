using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.EnsureSend;
using ZYSocket.RPC;
using System.Net.Sockets;
using System.Linq.Expressions;
using ZYSocket.share;
using System.Threading.Tasks.Schedulers;
using System.Threading.Tasks;

namespace ZYSocket.RPC.Server
{
    public class RPCUserInfo
    {
        public QueuedTaskScheduler Scheduler { get; set; }
        public SocketAsyncEventArgs Asyn { get; private set; }
        public RPC RPC_Call { get; set; }

        public ZYNetRingBufferPool  Stream { get; set; }

        public object UserToken { get; set; }


        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="data"></param>
        public virtual void BeginSendData(byte[] data)
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


        public void Disconn()
        {
            if (Asyn != null && Asyn.AcceptSocket != null)
                Asyn.AcceptSocket.Disconnect(false);
        }
           
        public T GetRPC<T>()
        {
            return RPC_Call.GetRPC<T>();
        }

        #region Reg Call


        /*
        public void CallAsyn<Mode>(Expression<Action<Mode>> action)
        {
            RPC_Call.CallAsyn<Mode>(action);
        }
        public void CallAsyn<Mode, Result>(Expression<Func<Mode, Result>> action, Action<AsynReturn> Callback)
        {
             RPC_Call.CallAsyn<Mode, Result>(action, Callback);
        }

        public void CallAsyn<Mode>(Expression<Action<Mode>> action, Action<AsynReturn> Callback)
        {
            RPC_Call.CallAsyn<Mode>(action, Callback);
        }

        public Result Call<Mode, Result>(Expression<Func<Mode, Result>> action)
        {
            return RPC_Call.Call<Mode, Result>(action);
        }

        public void Call<Mode>(Expression<Action<Mode>> action)
        {
            RPC_Call.Call<Mode>(action);
        }
        */
        #endregion


        public RPCUserInfo(SocketAsyncEventArgs asyn)
        {
            Scheduler = new QueuedTaskScheduler(4);
            RPC_Call = new RPC();          
            RPC_Call.CallBufferOutSend += RPC_OBJ_CallBufferOutSend;
            this.Asyn = asyn;
            Stream = new ZYNetRingBufferPool(1024 * 1024*2);//2MB

        }

        public RPCUserInfo(SocketAsyncEventArgs asyn,int maxSize)           
        {
            RPC_Call = new RPC();
            RPC_Call.CallBufferOutSend += RPC_OBJ_CallBufferOutSend;
            Stream = new ZYNetRingBufferPool(maxSize);
            this.Asyn = asyn;
        }

        void RPC_OBJ_CallBufferOutSend(byte[] data)
        {
            BeginSendData(data);
            //base.EnsureSend(data);
        }

    }
}
