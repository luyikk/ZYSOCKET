using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPCX.Service;
using System.Threading.Tasks;


namespace Server
{
   [RPCTAG("ITalkService")]
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

     
        [RPCMethod("IsLogIn")]
        public bool IsLogInX(string name)
        {
            var rpc= GetCurrentRPCUser();
            rpc.UserToken = name;

            UserList.Add(rpc);

            var client = rpc.GetRPC<IClient>();

            client.test(0);


            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
            stop.Start();

            Parallel.For(0, 10000, i =>
            {
                client.test(i);
            });

            stop.Stop();

            Console.WriteLine("test call ms:" + stop.ElapsedMilliseconds);

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
