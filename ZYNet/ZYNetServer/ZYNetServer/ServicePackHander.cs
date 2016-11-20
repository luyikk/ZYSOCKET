using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.Server;
using ZYSocket.share;
using ZYSocket.ZYNet.PACK;
using static ZYSocket.ZYNet.ServLOG;

namespace ZYSocket.ZYNet.Server
{
    internal class ServicePackHander : ICmdToCall
    {

        #region Only Static Object
        static object lockthis = new object();

        static ServicePackHander _My;

        public static ServicePackHander GetInstance()
        {
            lock (lockthis)
            {
                if (_My == null)
                    _My = new ServicePackHander();
            }

            return _My;
        }
        private ServicePackHander()
        {


        }
        #endregion


        public void Loading()
        {
            CmdToCallManager<ZYNetServer, ReadBytes, ZYNetSession>.GetInstance().AddPackerObj(this);
        }



        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="read"></param>
        /// <param name="session"></param>
        [CmdTypeOfAttibutes(-1000)]
        public void _RegSession(ZYNetServer Server, ReadBytes read, ZYNetSession session)
        {
            RegSession regSession;
            if (read.ReadObject<RegSession>(out regSession))
            {

                session.LANIP = regSession.LocalHost;
                session.Group = regSession.Group;
                if (!Server.SessionDiy.ContainsKey(session.Id))
                {
                    regSession.Id = session.Id;
                    regSession.IsSuccess = true;
                    regSession.Port = Server.RegPort;

                    Server.SessionDiy.AddOrUpdate(session.Id, session, (a, b) => session);
                    Server.Send(session, BufferFormat.FormatFCA(regSession));


                    AddSession add = new AddSession()
                    {
                        Id = regSession.Id
                    };

                    Server.SendAll(BufferFormat.FormatFCA(add));

                }
                else
                {
                    regSession.IsSuccess = false;
                    regSession.Msg = "User Id Is Have";
                    Server.Send(session, BufferFormat.FormatFCA(regSession));
                    Server.Service.Disconnect(session.Asyn.AcceptSocket);
                }

            }
        }

        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="read"></param>
        /// <param name="session"></param>
        [CmdTypeOfAttibutes(-1001)]
        public void _GetAllSession(ZYNetServer Server, ReadBytes read, ZYNetSession session)
        {
            GetAllSession pack;
            if (read.ReadObject<GetAllSession>(out pack))
            {
                var userlist = Server.SessionDiy.Values.Where(p => p.Group == session.Group && p.Id != session.Id);

                pack.UserIds = new List<long>();

                foreach (var item in userlist)
                {
                    pack.UserIds.Add(item.Id);
                }

                pack.IsSuccess = true;

                Server.Send(session, BufferFormat.FormatFCA(pack));
            }
        }

        /// <summary>
        /// 获取连接信息
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="read"></param>
        /// <param name="session"></param>
        [CmdTypeOfAttibutes(-1002)]
        public void _Connect(ZYNetServer Server, ReadBytes read, ZYNetSession session)
        {
            long ToId;
            if (read.ReadInt64(out ToId))
            {

                if (Server.SessionDiy.ContainsKey(ToId) && !string.IsNullOrEmpty(session.WANIP) && session.WANPort != 0)
                {
                    ZYNetSession toUser = Server.SessionDiy[ToId];

                    if (toUser.Group != session.Group)
                    {
                        return;
                    }

                    if (!session.WANIP.Equals(toUser.WANIP)) //如果不再一个局域网内
                    {
                        ConnectTo tmp = new ConnectTo()
                        {
                            Id = session.Id,
                            Host = session.WANIP,
                            Port = session.WANPort,
                            IsSuccess = true
                        };

                        Server.Send(toUser, BufferFormat.FormatFCA(tmp));
                    }
                    else //同局域网内
                    {
                        ConnectTo tmp = new ConnectTo()
                        {
                            Id = session.Id,
                            Host = session.LANIP,
                            Port = session.NatNextPort,
                            IsSuccess = true
                        };

                        Server.Send(toUser, BufferFormat.FormatFCA(tmp));
                    }
                }

            }
        }


        /// <summary>
        /// 反向获取连接信息
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="read"></param>
        /// <param name="session"></param>
        [CmdTypeOfAttibutes(-1003)]
        public void _LEFTConnect(ZYNetServer Server, ReadBytes read, ZYNetSession session)
        {
            long ToId;
            if (read.ReadInt64(out ToId))
            {
                if (Server.SessionDiy.ContainsKey(ToId) && !string.IsNullOrEmpty(session.WANIP) && session.WANPort != 0)
                {
                    ZYNetSession toUser = Server.SessionDiy[ToId];

                    if (toUser.Group != session.Group)
                    {
                        return;
                    }

                    if (!session.WANIP.Equals(toUser.WANIP)) //如果不再一个局域网内
                    {
                        LEFTConnect tmp = new LEFTConnect()
                        {
                            Id = session.Id,
                            Host = session.WANIP,
                            Port = session.WANPort,
                            IsSuccess = true
                        };

                        Server.Send(toUser, BufferFormat.FormatFCA(tmp));
                    }
                    else //同局域网内
                    {
                        LEFTConnect tmp = new LEFTConnect()
                        {
                            Id = session.Id,
                            Host = session.LANIP,
                            Port = session.NatNextPort,
                            IsSuccess = true
                        };

                        Server.Send(toUser, BufferFormat.FormatFCA(tmp));
                    }
                }
            }
        }

        [CmdTypeOfAttibutes(-1004)]
        public void _SetGroup(ZYNetServer Server, ReadBytes read, ZYNetSession session)
        {
            int group;

            if(read.ReadInt32(out group))
            {
                session.Group = group;
            }
        }

     

    }
}
