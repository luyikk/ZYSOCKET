using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.share;
using ZYSocket.ClientA;
using System.Net.Sockets;
using PACK;

namespace Client
{
   
    /// <summary>
    /// 主界面
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// 登入对话框
        /// </summary>
        LogOn logon;

        public Form1()
        {
            ReadBytesV2.ObjFormatType = BuffFormatType.protobuf;
            BufferFormatV2.ObjFormatType = BuffFormatType.protobuf;

            if (SocketManager.client.ConnectionTo(RConfig.ReadString("Host"), RConfig.ReadInt("Port"))) //连接到服务器
            {
                logon = new LogOn();

                logon.ShowDialog(); //显示登入界面

                if (!logon.Logins) //如果登入界面关闭那么检查是否登入成功如果没有登入成功 则关闭程序
                {
                    Close();
                }

                SocketManager.Disconnection += new ExceptionDisconnection(client_Disconnection); //注册断开事件
                SocketManager.DataInput += new DataOn(client_DataOn);//注册数据包输入事件
                InitializeComponent();

            }
            else
            {
                MessageBox.Show("无法连接服务器"); //无法连接 提示 并关闭
                Close();
            }
        }

        void client_DataOn(byte[] Data)
        {
            ReadBytesV2 read = new ReadBytesV2(Data);

            int length;
            int cmd;

            if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && length == read.Length)
            {
                PACKTYPE cmds=(PACKTYPE)cmd;
                switch (cmds)
                {
                   
                   case PACKTYPE.DataRes:
                        DATARES dox;
                        if (read.ReadObject<DATARES>(out dox)) //获取服务器发送过来的 DATASET 
                        {                           
                            if (dox != null)
                            {
                                this.BeginInvoke(new EventHandler((o, x) =>
                                {
                                    DATARES nn = o as DATARES;

                                    if (nn != null)
                                    {
                                        switch (nn.Type)
                                        {
                                            case 1:
                                                {
                                                    foreach (string p in nn.Res)
                                                    {
                                                        this.richTextBox1.AppendText(p + "\r\n");
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }), dox);
                            }
                        }
                        break;

                }
            }

        }

        void client_Disconnection(string message)
        {
            MessageBox.Show(message); //断开显示消息 并关闭窗口
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DATA temp = new DATA() //发送一个DATASET请求
            {
                CMD="GET"
            };

            SocketManager.client.SendTo(BufferFormatV2.FormatFCA(temp));

        }
     
    }
}
