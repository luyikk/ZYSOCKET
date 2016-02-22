using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZYSocket.share;
using ZYSocket.RPCX.Client;

namespace RPCTalkClientWin
{
    [RPCTAG("IClient")]
    public partial class WinClient : Form
    {
        public WinClient()
        {
            InitializeComponent();          
        }

        private void WinClient_Load(object sender, EventArgs e)
        {
            if (!TalkService.Connect(RConfig.ReadString("Host"), RConfig.ReadInt("Port")))
            {
                MessageBox.Show("无法连接服务器");
                Application.Exit();
                return;
            }

            TalkService.Client.RegModule(this);
            LogAction.LogOut += LogAction_LogOut;
            LogOn tmp = new LogOn();
            tmp.ShowDialog();

            if(!tmp.IsLogOn)
            {
                Application.Exit();                
            }

                       
        }

        private void LogAction_LogOut(string msg, LogType type)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                this.richTextBox1.AppendText(msg + "\r\n");

            }));
        }

        [RPCMethod]
        public void UpdateUserList()
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                var userlist = TalkService.Client.GetRPC<TalkServer>().GetAllUser();

                this.listBox1.Items.Clear();

                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("全部用户");


                foreach (var item in userlist)
                {
                    this.listBox1.Items.Add(item);
                    this.comboBox1.Items.Add(item);
                }
            }));
        }

        [RPCMethod]
        public void MessageShow(string msg)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                this.richTextBox1.AppendText(msg + "\r\n");

            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(this.comboBox1.Text=="全部用户")
            {
                TalkService.Client.GetRPC<TalkServer>().SendAllMessage(this.textBox1.Text);
                this.textBox1.Text = "";
            }else
            {
                TalkService.Client.GetRPC<TalkServer>().SendToMessage(this.comboBox1.Text, this.textBox1.Text);
                this.textBox1.Text = "";

            }
        }
    }
}
