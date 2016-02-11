using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Server;
namespace Server
{
    public class TalkService:RPCObject
    {



        public int Add(int a, int b)
        {
            return a + b;
        }



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
                    my.AsynCall(()=>api.UserTalk(my.UserToken.ToString(), msg));                   
                }
            }
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
