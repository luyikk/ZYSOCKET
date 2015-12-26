using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.EnsureSend;
using ZYSocket.RPC;
using System.Net.Sockets;
using System.Linq.Expressions;
using ZYSocket.share;

namespace ZYSocket.RPC.Server
{
    public class RPCUserInfo:ZYEnsureSend
    {
        public RPC RPC_Call { get; set; }

        public ZYNetRingBufferPool  Stream { get; set; }

        public object UserToken { get; set; }


        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int OutTime { get { return RPC_Call.OutTime; } set { RPC_Call.OutTime = value; } }


        public void Disconn()
        {
            if (Asyn != null && Asyn.AcceptSocket != null)
                Asyn.AcceptSocket.Disconnect(false);
        }


        #region Reg Call



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

        #endregion


        public RPCUserInfo(SocketAsyncEventArgs asyn):base(asyn,1024*64)
        {
            RPC_Call = new RPC();
            RPC_Call.CallBufferOutSend += RPC_OBJ_CallBufferOutSend;
            Stream = new ZYNetRingBufferPool(1024 * 64);//64K

        }

        public RPCUserInfo(SocketAsyncEventArgs asyn,int maxSize)
            : base(asyn, maxSize)
        {
            RPC_Call = new RPC();
            RPC_Call.CallBufferOutSend += RPC_OBJ_CallBufferOutSend;
            Stream = new ZYNetRingBufferPool(maxSize);
        }

        void RPC_OBJ_CallBufferOutSend(byte[] data)
        {
            base.EnsureSend(data);
        }

    }
}
