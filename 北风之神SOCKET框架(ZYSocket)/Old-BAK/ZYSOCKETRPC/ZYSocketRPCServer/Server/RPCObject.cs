using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace ZYSocket.RPC.Server
{
    public class RPCObject
    {
        protected RPCUserInfo GetCurrentRPCUser()
        {
            return CallContext.GetData("Current") as RPCUserInfo;
        }


        public virtual void ClientDisconnect(RPCUserInfo userInfo)
        {
            return;
        }

        public Task AsynCall(Action action)
        {
            return Task.Factory.StartNew(action);
        }

        public Task<Result> AsynCall<Result>(Func<Result> action)
        {
            return Task.Factory.StartNew<Result>(action);
        }

    }
}
