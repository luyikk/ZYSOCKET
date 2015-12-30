using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace ZYSocket.RPC.Client
{
    public class RPCClientObj
    {
        protected RPCClient GetCurrentRPObj()
        {
            return CallContext.GetData("Current") as RPCClient;
        }
    }
}
