using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.RPC.Server;
using System.Threading;

namespace RPCTalkServer
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public RPCUserInfo RPCSession { get; set; }


        public override string ToString()
        {
            return UserName.ToString();
        }

    }


    public class TalkServer:RPCObject
    {

        public event EventHandler<ConcurrentDictionary<string, UserInfo>> UpdateUserList;

        public TalkServer()
        {
            UserList = new ConcurrentDictionary<string, UserInfo>();
        }

        /// <summary>
        /// 使用ConcurrentDictionary 在多线程访问的时候 处理非常方便，牺牲点性能可以略微不计
        /// </summary>
        public ConcurrentDictionary<string,UserInfo> UserList { get; set; }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        public bool LogOn(string userName,string passWord)
        {

            //-------检查用户名密码-----
            //--------错误返回false-----


            var RPCSession = GetCurrentRPCUser();

            UserInfo tmp = new UserInfo()
            {
                UserName=userName,
                RPCSession=RPCSession
            };

            RPCSession.UserToken = tmp;

           
            if(!UserList.ContainsKey(userName))
            {
                SpinWait.SpinUntil(() => UserList.TryAdd(userName, tmp)); //使用 SpinWait 在解决多线程插入的时候返回false可以spinwait，提高性能和确保插入
            }
            else
            {
                UserList[userName].RPCSession.Disconn();

                UserInfo outuser;
                SpinWait.SpinUntil(() => UserList.TryRemove(userName, out outuser));//删除
                SpinWait.SpinUntil(() => UserList.TryAdd(userName, tmp)); //使用 SpinWait 在解决多线程插入的时候返回false可以spinwait，提高性能和确保插入
            }

            UpdateUserList(this, UserList);


            SendUpdateUserlist();

            return true;
        }



        private void SendUpdateUserlist()
        {
            foreach (var item in UserList.Values) //通知其他人更新用户列表
            {
                item.RPCSession.AsynCall(() =>
                {
                    item.RPCSession.GetRPC<WinClient>().UpdateUserList();
                });
            }
        }


        /// <summary>
        /// 用户断开服务器触发
        /// </summary>
        /// <param name="userInfo"></param>
        public override void ClientDisconnect(RPCUserInfo userInfo)
        {
            if(userInfo.UserToken!=null)
            {
                UserInfo user = userInfo.UserToken as UserInfo;
                if (user != null)
                {
                    string key = user.UserName;
                    SpinWait.SpinUntil(() => UserList.TryRemove(key, out user)); //删除列表
                }
            }

            UpdateUserList(this, UserList);
            SendUpdateUserlist();
        }


        public void SendAllMessage(string msg)
        {
            UserInfo user = GetCurrentRPCUser().UserToken as UserInfo;

            foreach (var item in UserList.Values)
            {
                item.RPCSession.AsynCall(() =>
                {
                    item.RPCSession.GetRPC<WinClient>().MessageShow(user.UserName + ":" + msg);
                });
            }
        }

        public void SendToMessage(string username, string msg)
        {
            UserInfo user = GetCurrentRPCUser().UserToken as UserInfo;

            var userinfo= UserList.Values.FirstOrDefault(p => p.UserName == username);

            if (userinfo != null)
                userinfo.RPCSession.AsynCall(() => userinfo.RPCSession.GetRPC<WinClient>().MessageShow(user.UserName + " 对你说:" + msg));
        }

        public List<string> GetAllUser()
        {
            UserInfo user = GetCurrentRPCUser().UserToken as UserInfo;

            var list = UserList.Keys.ToList();

            list.Remove(user.UserName);

            return list;

        }


    }
}
