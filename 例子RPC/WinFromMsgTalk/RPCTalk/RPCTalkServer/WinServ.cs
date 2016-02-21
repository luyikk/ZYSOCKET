using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZYSocket.RPC;
using ZYSocket.RPC.Server;

namespace RPCTalkServer
{
    public partial class WinServ : Form
    {
        RPCServer Server;

        TalkServer Service = new TalkServer();

        public WinServ()
        {
            InitializeComponent();

            Server = new RPCServer();
            Server.MsgOut += Server_MsgOut1;
            Server.IsCanConn += Server_IsCanConn;
            //Server.ReadOutTime = 5000;
            Service.UpdateUserList += Service_UpdateUserList;
          
            Server.RegServiceModule(Service);
        }

        private void Service_UpdateUserList(object sender, System.Collections.Concurrent.ConcurrentDictionary<string, UserInfo> e)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                this.listBox1.Items.Clear();

                foreach (var item in e.Values)
                {
                    this.listBox1.Items.Add(item);
                }
            }));
        }

        private void Server_MsgOut1(string msg, MsgOutType logType)
        {
            this.BeginInvoke(new EventHandler((a, b) => this.richTextBox1.AppendText("["+logType+"] "+msg + "\r\n")));
        }

        /// <summary>
        /// IP限制
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        private bool Server_IsCanConn(System.Net.IPEndPoint ipaddress)
        {
            return true;
        }

    

        private void button1_Click(object sender, EventArgs e)
        {
            Server.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Server.Pause();
        }

        private void dissconThisUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.listBox1.SelectedItem!=null)
            {
                UserInfo user = this.listBox1.SelectedItem as UserInfo;

                if (user != null)
                    user.RPCSession.Disconn();
            }
        }
    }
}
