using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.ZYNet.Server;
using ZYSocket.share;
using DrawBoardPACK;
using System.Drawing;
using ZYSocket.ZYNet.Client;

namespace DrawBoardServer
{
    class Program
    {
        static ZYNetServer Server;

        static List<UserInfo> UserList;

        static Bitmap Map;
        static Graphics gs;

        static void Main(string[] args)
        {
            Map = new Bitmap(848, 609);
            gs = Graphics.FromImage(Map);
            UserList = new List<UserInfo>();
            Server = ZYNetServer.GetInstance();
            Server.UserConnectAuthority += Server_UserConnectAuthority;
            Server.UserDisconnect += Server_UserDisconnect;
            Server.UserDataInput += Server_UserDataInput;
            Server.Start();

            Console.WriteLine("服务器以启动");
            Console.ReadLine();
        }            

        private static void Server_UserDisconnect(ZYSocket.ZYNet.ZYNetSession session)
        {
            var x = session?.Asyn?.AcceptSocket?.RemoteEndPoint;

            if(x!=null)
            {
                Console.WriteLine(x.ToString() + " Disconnect");
                session.UserToken = null;
            }

        }

        private static bool Server_UserConnectAuthority(System.Net.IPAddress address, int port)
        {
            Console.WriteLine(address.ToString() + ":" + port + " Connect");
            return true;
        }

        private static void Server_UserDataInput(ZYSocket.ZYNet.ZYNetSession session, byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && read.ReadInt32(out cmd) && read.Length == lengt)
            {
                switch(cmd)
                {
                    case 1000:
                        {
                            LogOn logon;

                            if(read.ReadObject<LogOn>(out logon))
                            {
                                UserInfo tmp = new UserInfo()
                                {
                                    Name = logon.UserName,
                                    Session = session
                                };

                                session.UserToken = tmp;

                                UserList.Add(tmp);

                                logon.Success = true;

                                Server.SendDataToClient(session, BufferFormat.FormatFCA(logon));
                                Console.WriteLine(tmp.Name + "登入");
                            }
                        }
                        break;
                    case 2000:
                        {
                            if (session.UserToken == null)
                                return;

                            DrawPoint tmp;

                            if(read.ReadObject<DrawPoint>(out tmp))
                            {
                                if (Map == null)
                                {
                                    Map = new Bitmap(848, 609);
                                    gs = Graphics.FromImage(Map);
                                }

                                Brush br = new SolidBrush(Color.FromArgb(tmp.Color));

                                gs.FillEllipse(br, tmp.X, tmp.Y, 2, 2);
                                gs.Flush();
                                gs.Save();
                            }
                        }
                        break;
                    case 3000:
                        {
                            if (session.UserToken == null)
                                return;

                            ClecrColor tmp;
                            if (read.ReadObject<ClecrColor>(out tmp))
                            {
                                gs.Clear(Color.FromArgb(tmp.Color));
                                gs.Flush();
                                gs.Save();
                            }
                        }
                        break;
                    case 4000:
                        {
                            if (session.UserToken == null)
                                return;

                            SaveImg tmp;
                            if (read.ReadObject<SaveImg>(out tmp))
                            {
                                string path = (new System.IO.FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)).Directory.FullName + "\\SAVE\\";

                                if(!System.IO.Directory.Exists(path))
                                {
                                    System.IO.Directory.CreateDirectory(path);
                                }


                                Map.Save(path + tmp.FileName);

                            }
                        }
                        break;

                }
            }

        }


    }
}
