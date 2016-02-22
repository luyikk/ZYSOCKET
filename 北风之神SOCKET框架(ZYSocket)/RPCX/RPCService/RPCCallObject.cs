using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ZYSocket.RPCX.Service
{
    public class RPCCallObject
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
            return Task.Factory.StartNew(()=>
            {
                try
                {
                    action();
                }
                catch (Exception er)
                {
                    var userinfo = GetCurrentRPCUser();

                    if(userinfo!=null&&userinfo.Asyn!=null&&userinfo.Asyn.AcceptSocket!=null)
                    {
                        LogAction.Err(userinfo.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "->" + er.ToString());
                    }
                }
            });
        }

        public Task<Result> AsynCall<Result>(Func<Result> action)
        {
            return Task.Factory.StartNew<Result>(()=>
            {
                try
                {
                    return action();
                }
                catch (Exception er)
                {
                    var userinfo = GetCurrentRPCUser();

                    if (userinfo != null && userinfo.Asyn != null && userinfo.Asyn.AcceptSocket != null)
                    {
                        LogAction.Err(userinfo.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "->" + er.ToString());
                    }

                    return default(Result);
                }

            });
        }
    }
}
