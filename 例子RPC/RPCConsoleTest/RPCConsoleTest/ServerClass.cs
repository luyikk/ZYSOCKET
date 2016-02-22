using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPCX.Service;
using System.Timers;

namespace RPCConsoleTest
{







    public class ServerClass:RPCCallObject
    {
        public List<RPCUserInfo> UserList { get; set; }

        public ServerClass()
        {
            UserList = new List<RPCUserInfo>();
            Timer time = new Timer(1000);
            time.Elapsed += Time_Elapsed;
            time.Start();

            Timer time2 = new Timer(5000);
            time2.Elapsed += Time2_Elapsed;
            time2.Start();
        }

        private void Time2_Elapsed(object sender, ElapsedEventArgs e)
        {
            Random r = new Random();

            foreach (var item in UserList)
            {

                AsynCall<long>(() => item.GetRPC<IClientCall>().Add(r.Next(), r.Next())).ContinueWith(res =>
                  {                      
                          Console.WriteLine(item.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "\t " + (item.UserToken as UserInfo).UserName + " 计算结果为:" + res.Result);                     
                  });            
            }
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e) //每个1秒获取客户端的计算机名称
        {
            foreach (var item in UserList)
            {
                DateTime time= item.GetRPC<IClientCall>().GetClientDateTime();

                Console.WriteLine(item.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "\t " + (item.UserToken as UserInfo).UserName + " 的时间为" + time);
            }
        }



        private bool CheckUser()
        {
            var rpc = GetCurrentRPCUser();
         
            if (rpc.UserToken != null&& UserList.Contains(rpc))
            {
                return true;
            }
          

            return false;
        }


        public bool LogOn(string username, string password)
        {
            var rpc= GetCurrentRPCUser();

            UserInfo tmp = new UserInfo()
            {
                UserName=username
            };

            rpc.UserToken = tmp;

            UserList.Add(rpc);

            Console.WriteLine(username + " 登入成功");

       

            return true;

        }


        public DateTime GetServerTime()
        {
            if (CheckUser())
            {
                return DateTime.Now;
            }

            GetCurrentRPCUser().Disconnect(); //没登入断开连接           

            return DateTime.MinValue;
        }

        public void OutRandom(out int value)
        {
            Random r = new Random();
            value=r.Next();
        }

        public Data Return(Data ins)
        {
            ins.Name = "OK";            
            ins.Value++;
            return ins;
        }


        public void SendAll(string msg)
        {
            var rpc = GetCurrentRPCUser();

            string msgx = (rpc.UserToken as UserInfo).UserName + " :" + msg;

            foreach (var item in UserList)
            {
                AsynCall(() => item.GetRPC<IClientCall>().ShowMsg(msgx));
            }

        }

        public int Add(int a, int b)
        {
            return a + b;
        }


        public string[] array(string[] z)
        {
          
            return z;

        }

        public int TestOutAndRef(out int a, ref int b)
        {
            a = b;
            b = b + 1;

            return a + b;
        }

        public override void ClientDisconnect(RPCUserInfo userInfo)
        {
            if (UserList.Contains(userInfo))
            {
                UserList.Remove(userInfo);
                Console.WriteLine((userInfo.UserToken as UserInfo).UserName + " 退出");
            }
        }
    }
}
