using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Server;
using System.Timers;

namespace RPCConsoleTest
{

    public interface ClientCall
    {
        DateTime GetClientDateTime();
        long Add(long a, long b);

        int RecComputer(int i);
    }





    public class ServerClass:RPCObject
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

                item.CallAsyn<ClientCall>(p => p.Add(r.Next(), r.Next()), new Action<ZYSocket.RPC.AsynReturn>((x) =>
                    {
                        Console.WriteLine(item.Asyn.AcceptSocket.RemoteEndPoint.ToString() + "\t " + (item.UserToken as UserInfo).UserName + " 计算结果为:" + x.Return);

                    }));              
            }
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e) //每个1秒获取客户端的计算机名称
        {
            foreach (var item in UserList)
            {
                DateTime time= item.Call<ClientCall, DateTime>(p => p.GetClientDateTime());

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
            var rpc = GetCurrentRPCUser(); //没登入断开连接
            rpc.Disconn(); 

            return DateTime.MinValue;
        }

        public void OutRandom(out int value)
        {
            Random r = new Random();
            value=r.Next();
        }

        public Data Return(Data ins)
        {
            ins.Name += "Ok";
            return ins;
        }


        public int RecComputer(int i)
        {
            if (i < 2)
                return i;

            i--;

            var rpc = GetCurrentRPCUser();

            i = rpc.Call<ClientCall, int>(p => p.RecComputer(i));

            return i;

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
