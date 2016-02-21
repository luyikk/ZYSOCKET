using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Server;
namespace Server
{
    public class TalkService:RPCObject
    {
        List<RPCUserInfo> UserList
        {
            get; set;
        }

        public TalkService()
        {
            UserList = new List<RPCUserInfo>();
        }

        public bool IsLogIn(string name)
        {
            var rpc= GetCurrentRPCUser();
            rpc.UserToken = name;

            UserList.Add(rpc);

            return true;
        }


        public void SendALL(string msg)
        {
            if (UserList.Contains(GetCurrentRPCUser()))
            {
                var my = GetCurrentRPCUser();
                var api = my.GetRPC<Client>();
              

                foreach (var item in UserList)
                {
                    AsynCall(() => { api.UserTalk(my.UserToken.ToString(), msg); });
                }
            }
        }

        public int value(int a,int b)
        {
            return a + b;
        }




        public override void ClientDisconnect(RPCUserInfo userInfo)
        {
            if (UserList.Contains(userInfo))
            {
                UserList.Remove(userInfo);
            }

        }
    }
}
