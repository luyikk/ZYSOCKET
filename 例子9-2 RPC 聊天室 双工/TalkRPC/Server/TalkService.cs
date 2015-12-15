using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC;
using ZYSocket.share;
using System.Net.Sockets;

namespace Server
{
    public class TalkService : RPCObject
    {
        public List<UserInfo> UserList { get; set; }

        public TalkService()
        {
            UserList = new List<UserInfo>();
        }


        public override void ClientDisconnect(SocketAsyncEventArgs socketasyn)
        {
            if (socketasyn.UserToken != null)
            {
                UserInfo user = socketasyn.UserToken as UserInfo;

                if (UserList.Contains(user))
                {
                    UserList.Remove(user);

                    foreach (var item in UserList)
                    {
                        BufferFormatV2 buff = new BufferFormatV2(1);
                        Send(item.Asyn, buff.Finish());
                    }
                }
            }
        }

        public bool LogOn(string nickname,out string message)
        {
            SocketAsyncEventArgs asyn = GetCurrentSocketAsynEvent();

            if (asyn!=null)
            {
                UserInfo user = asyn.UserToken as UserInfo;

                if (user != null)
                {
                    user.UserName = nickname;
                    message = "登入成功";

                    if (!UserList.Contains(user))
                    {
                        UserList.Add(user);

                    }


                    foreach (var item in UserList)
                    {
                        if (item != user)
                        {
                            BufferFormatV2 buff=new BufferFormatV2(1);
                            Send(item.Asyn, buff.Finish());
                        }

                    }



                    return true;
                }

            }
            message = "登入失败";
            return false;
        }

        public List<string> GetAllUser()
        {
            List<string> list = new List<string>();

            foreach (var item in UserList)
            {
                list.Add(item.UserName);
            }

            return list;
        }

        public bool MessageTalk(string nick, string context)
        {
            if (string.IsNullOrEmpty(nick) || nick == "所有人")
            {
                UserInfo user = GetCurrentSocketAsynEvent().UserToken as UserInfo;

                foreach (var item in UserList)
                {
                    BufferFormatV2 buff = new BufferFormatV2(2);
                    buff.AddItem(DateTime.Now.ToString("HH:mm:ss") + "\t" + user.UserName + " 说:" + context);
                    Send(item.Asyn, buff.Finish());
                }

                return true;
            }
            else
            {
                UserInfo user = UserList.Find(p => p.UserName == nick);

                if (user != null)
                {
                    BufferFormatV2 buff = new BufferFormatV2(2);
                    buff.AddItem(DateTime.Now.ToString("HH:mm:ss") + "\t" + user.UserName + " 对你说:" + context);
                    Send(user.Asyn, buff.Finish());

                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

    }

}
