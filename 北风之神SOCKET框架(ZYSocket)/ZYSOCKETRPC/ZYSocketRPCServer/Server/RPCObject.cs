using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

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

    }
}
