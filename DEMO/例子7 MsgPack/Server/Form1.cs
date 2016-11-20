using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.Server;
using ZYSocket.share;
using System.Net;
using System.Net.Sockets;
using PACK;

namespace Server
{
    public partial class Form1 : Form
    {
        ZYSocketSuper server;

        List<UserManager> userlist = new List<UserManager>();

        public Form1()
        {
          

            InitializeComponent();

            ReadBytesV2.ObjFormatType = BuffFormatType.MsgPack;
            BufferFormatV2.ObjFormatType = BuffFormatType.MsgPack;


            server = new ZYSocketSuper();
            server.BinaryInput = new BinaryInputHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
        }


        public void LogOut(string str)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
                {
                    this.richTextBox1.AppendText(str + "\r\n");
                }));

        }


        /// <summary>
        /// 用户断开代理（你可以根据socketAsync 读取到断开的
        /// </summary>
        /// <param name="message">断开消息</param>
        /// <param name="socketAsync">断开的SOCKET</param>
        /// <param name="erorr">错误的ID</param>
        void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            LogOut(message);

            if (socketAsync.UserToken != null)
            {
                UserManager user = socketAsync.UserToken as UserManager;

                if (user != null)
                {
                    userlist.Remove(user);
                }

                socketAsync.UserToken = null;
            }
            socketAsync.AcceptSocket.Close();
            socketAsync.AcceptSocket.Dispose();
        }
        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            LogOut(string.Format("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString()));
            socketAsync.UserToken = new ZYNetRingBufferPoolV2();
            return true;
        }



        /// <summary>
        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            try
            {


                ZYNetRingBufferPoolV2 stream = socketAsync.UserToken as ZYNetRingBufferPoolV2;

                if (stream != null)
                {
                    //最新的数据包整合类

                    stream.Write(data);

                    byte[] datax;
                    while (stream.Read(out datax))
                    {
                        LogOnPack(datax, socketAsync);
                    }
                }
                else
                {
                    UserManager userinfo = socketAsync.UserToken as UserManager;

                    userinfo.Stream.Write(data);

                    byte[] datax;
                    while (userinfo.Stream.Read(out datax))
                    {
                        DataOn(datax, userinfo);
                    }
                }

            }
            catch (Exception er)
            {
                LogOut(er.ToString());
            }
        }

        void LogOnPack(byte[] data, SocketAsyncEventArgs asyn)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt; //数据包长度,用于验证数据包的完整性
            int cmd; //数据包命令类型

            //注意这里一定要这样子写,这样子可以保证所有你要度的数据是完整的,如果读不出来 Raed方法会返回FALSE,从而避免了错误的数据导致崩溃
            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {  //read.Read系列函数是不会产生异常的

                //根据命令读取数据包
                PACKTYPE cmdType=(PACKTYPE)cmd;

                switch (cmdType)
                {
                    case PACKTYPE.LogOn:
                        {
                            LOGON _logon;

                            if (read.ReadObject<LOGON>(out _logon))
                            {
                                //DOTO:验证用户

                                LogOut(_logon.username + "  登入成功");

                                UserManager tmp = new UserManager()
                                {
                                    Asyn=asyn,
                                    Stream=new ZYNetRingBufferPoolV2(),
                                    UserName=_logon.username
                                };

                                asyn.UserToken = tmp;

                                userlist.Add(tmp);


                                LOGONRES senddata = new LOGONRES()
                                {
                                    IsLogOn=true,
                                    Msg="登入成功"
                                };

                                server.SendData(asyn.AcceptSocket, BufferFormatV2.FormatFCA(senddata));

                            }
                        }
                        break;
                }

            }

        }

        void DataOn(byte[] data, UserManager e)
        {
           
                //建立一个读取数据包的类 参数是数据包
                //这个类的功能很强大,可以读取数据包的数据,并可以把你发送过来的对象数据,转换对象引用
                ReadBytesV2 read = new ReadBytesV2(data);

                int lengt; //数据包长度,用于验证数据包的完整性
                int cmd; //数据包命令类型

                //注意这里一定要这样子写,这样子可以保证所有你要度的数据是完整的,如果读不出来 Raed方法会返回FALSE,从而避免了错误的数据导致崩溃
                if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
                {  //read.Read系列函数是不会产生异常的

                    PACKTYPE cmdType = (PACKTYPE)cmd;

                    //根据命令读取数据包
                    switch (cmdType)
                    {
                        case PACKTYPE.Data:
                            {
                                DATA tmp;

                                if (read.ReadObject<DATA>(out tmp))
                                {
                                    LogOut(e.UserName + "数据命令:" + tmp.CMD);

                                    switch (tmp.CMD)
                                    {
                                        case "GET":
                                            {
                                                DATARES _var1 = new DATARES()
                                                {
                                                    Type = 1,
                                                    Res = new List<string>()
                                                };

                                                _var1.Res.Add("数据1");
                                                _var1.Res.Add("数据2");
                                                _var1.Res.Add("数据3");
                                                _var1.Res.Add("数据4");
                                                _var1.Res.Add("数据5");

                                                

                                                server.SendData(e.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(_var1));
                                            }
                                            break;

                                    }
                                }

                            }
                            break;
                    }

                }

          
        }

        bool IsSTART;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsSTART)
            {
                server.Start();
                IsSTART = true;
                this.button1.Text = "暂停";
            }
            else
            {
                server.Stop();
                IsSTART = false;
                this.button1.Text = "开始";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DATARES _var1 = new DATARES()
            {
                Type = 1,
                Res = new List<string>()
            };

            _var1.Res.Add("群发......");

            foreach (var p in userlist)
            {
                server.SendData(p.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(_var1));
            }
        }
    }
}
