using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.share;
using PACK;
namespace Client
{
    /// <summary>
    /// 登入界面
    /// </summary>
    public partial class LogOn : Form
    {
        /// <summary>
        /// 登入成功为TRUE
        /// </summary>
        public bool Logins { get; set; }


        public LogOn()
        {
            //注册数据包输入事件
            SocketManager.DataInput += new ZYSocket.ClientA.DataOn(SocketManager_DataInput);
            InitializeComponent();
            //测试关闭事件
            this.Closed += new EventHandler(LogOn_Closed);
            Logins = false;
        }

        void LogOn_Closed(object sender, EventArgs e)
        {
            //删除数据包输入事件
            SocketManager.DataInput -= new ZYSocket.ClientA.DataOn(SocketManager_DataInput);
        }

        /// <summary>
        /// 输入包输入处理
        /// </summary>
        /// <param name="Data"></param>
        void SocketManager_DataInput(byte[] Data)
        {
            ReadBytesV2 read = new ReadBytesV2(Data);

            int length;
            int cmd;

            if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && length == read.Length)
            {
                PACKTYPE cmds=(PACKTYPE)cmd;
                switch (cmds)
                {
                    case  PACKTYPE.LogOnRes: //如果是Message类型数据包
                        LOGONRES ms;
                        if (read.ReadObject<LOGONRES>(out ms))
                        {
                            
                            if (ms != null)
                            {
                                if (!ms.IsLogOn) //1登入失败
                                    MessageBox.Show(ms.Msg);
                                else if (ms.IsLogOn) //2登入成功
                                {
                                    Logins = true; //设置 LOGINS
                                    this.BeginInvoke(new EventHandler((o, p) => Close()));  //关闭窗口
                                   
                                }
                            }
                        }

                        break;
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LOGON temp = new LOGON() //发送一个登入数据包
            {
                username = this.textBox1.Text,
                password = this.textBox2.Text
            };

            SocketManager.client.SendTo(BufferFormatV2.FormatFCA(temp));
        }
    }
}
