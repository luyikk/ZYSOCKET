﻿using System;
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
                foreach (var item in UserList)
                {
                    item.GetRPC<Client>().UserTalk(GetCurrentRPCUser().UserToken.ToString(), msg);
                    throw new Exception("dd");
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
