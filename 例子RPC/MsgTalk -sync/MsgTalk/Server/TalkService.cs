using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPCX.Service;
namespace Server
{
    public class TalkService:RPCCallObject
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

            Console.WriteLine("ClientID:" + rpc.GetRPC<IClient>().GetID());    

            return true;
        }


        public void SendALL(string msg)
        {
            if (UserList.Contains(GetCurrentRPCUser()))
            {
                var my = GetCurrentRPCUser();
                            

                foreach (var item in UserList)
                {
                    item.GetRPC<IClient>().UserTalk(my.UserToken.ToString(), msg);
                }
            }
        }

        public int value(int a,int b)
        {
            return a + b;
        }


        public void notReturn(int a)
        {

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
