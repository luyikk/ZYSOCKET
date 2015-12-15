using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.share;
using ZYSocket.RPC.Client;
using Server;

namespace Client
{
    public partial class WinMain : Form
    {
        RPCClient client;
        public WinMain()
        {
            InitializeComponent();
        }

        private void WinMain_Load(object sender, EventArgs e)
        {
            LogOn tmp = new LogOn();

            tmp.ShowDialog();

            if (!string.IsNullOrEmpty(tmp.nickName))
            {
                client = new RPCClient();
                if (client.Connection("127.0.0.1", 9562))
                {
                    client.Disconn += new ZYSocket.ClientB.ClientMessageInputHandler(client_Disconn);
                    client.DataOn += new ZYSocket.ClientB.ClientBinaryInputHandler(client_DataOn);

                    string msg = null; ;
                    if (client.Call<TalkService, bool>(p => p.LogOn(tmp.nickName, out msg)))
                    {
                        this.richTextBox1.AppendText(msg+"\r\n");
                        this.Text = tmp.nickName;

                        GetAllUser();

                        return;
                    }
                   
                }

            }


            this.Close();
        }

        public void GetAllUser()
        {
            List<string> userlist = client.Call<TalkService, List<string>>(p => p.GetAllUser());

            this.listView1.Items.Clear();

            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("所有人");

            foreach (var item in userlist)
            {
                this.listView1.Items.Add(new ListViewItem(item));
                this.comboBox1.Items.Add(item);
            }
            

          
        }


        void client_DataOn(byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 1:
                        {
                            this.BeginInvoke(new EventHandler((a, b) =>
                                {
                                    GetAllUser();
                                }));
                        }
                        break;
                    case 2:
                        {
                            string msg;

                            if (read.ReadString(out msg))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                {
                                    this.richTextBox1.AppendText(msg + "\r\n");
                                }));
                            }

                        }
                        break;
                }

            }
        }

        void client_Disconn(string message)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
                {
                    MessageBox.Show(message);
                    this.Close();
                }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!client.Call<TalkService, bool>(p => p.MessageTalk(this.comboBox1.Text, this.textBox1.Text)))
            {
                this.richTextBox1.AppendText("发送失败");
            }
          
        }
    }
}
