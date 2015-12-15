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
using BuffLibrary;

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
            ReadBytes read = new ReadBytes(Data);

            int length;
            int cmd;

            if (read.ReadInt32(out length) && read.ReadInt32(out cmd) && length == read.Length)
            {
                switch (cmd)
                {
                    case 800: //PING命令
                        {
                            Ping p;
                            if (read.ReadObject<Ping>(out p))
                            {                              
                                if(p!=null)
                                this.BeginInvoke(new EventHandler((o, x) =>
                                    {
                                        Ping nn = o as Ping;

                                        if (nn != null)
                                        {
                                            toolStripStatusLabel1.Text = string.Format("Ping:{0} ({1})", //计算并显示PING
                                                (DateTime.Now - nn.UserSendTime).Milliseconds,
                                                (DateTime.Now - nn.ServerReviceTime).Milliseconds);
                                        }
                                    }),p);
                            }
                        }
                        break;
                    case 1002:
                        ReadDataSet dox;
                        if (read.ReadObject<ReadDataSet>(out dox)) //获取服务器发送过来的 DATASET 
                        {                           
                            if (dox != null)
                            {
                                this.BeginInvoke(new EventHandler((o, x) =>
                                {
                                    ReadDataSet nn = o as ReadDataSet;

                                    if (nn != null)
                                    {
                                        
                                        this.dataGridView1.DataSource = nn.Data; //绑定到视图
                                        this.dataGridView1.Update();
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
            this.BeginInvoke(new EventHandler((a,b)=>this.Close() ));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReadDataSet temp = new ReadDataSet() //发送一个DATASET请求
            {
                TableName = "table1"
            };

            SocketManager.client.SendTo(BufferFormat.FormatFCA(temp));

        }

        private void timer1_Tick(object sender, EventArgs e) //一个TIMER 每过一段事件就 发送一个PING包
        {
            Ping temp = new Ping()
            {
                UserSendTime = DateTime.Now
            };

            SocketManager.client.SendTo(BufferFormat.FormatFCA(temp)); 

        }
    }
}
