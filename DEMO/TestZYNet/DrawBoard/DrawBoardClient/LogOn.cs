using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZYSocket.ZYNet.Client;
using DrawBoardPACK;

namespace DrawBoardClient
{
    public partial class LogOn : Form
    {

        public ZYNetClient Client { get; set; }

        public bool IsLogOn { get; set; }

        public string Names { get; set; }

        public LogOn(ZYNetClient client)
        {
            InitializeComponent();
            Client = client;
            Client.DataInput += Client_DataInput;
            Client.ServerDisconnect += Client_ServerDisconnect;
            this.Closed += LogOn_Closed;

           
        }

        private void LogOn_Closed(object sender, EventArgs e)
        {
            Client.DataInput -= Client_DataInput;
            Client.ServerDisconnect -= Client_ServerDisconnect;
        }

        private void Client_ServerDisconnect(string message)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                MessageBox.Show(message);
                Application.Exit();
            }));
           
        }

        private void Client_DataInput(long Id, byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int cmd;
            int lengt;
            if(read.ReadInt32(out lengt)&&read.ReadInt32(out cmd)&&lengt==read.Length)
            {
                switch(cmd)
                {
                    case 1000:
                        {
                            if(Id==0)
                            {
                                DrawBoardPACK.LogOn tmp;

                                if(read.ReadObject<DrawBoardPACK.LogOn>(out tmp))
                                {
                                    if(tmp.Success)
                                    {
                                        MessageBox.Show("登入成功");
                                        IsLogOn = true;
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            this.Close();
                                        }));

                                        Names = tmp.UserName;
                                    }
                                    else
                                    {
                                        MessageBox.Show("登入失败");
                                    }
                                }
                            }
                        }
                        break;


                }
            }
        }

        private void LogOn_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawBoardPACK.LogOn tmp = new DrawBoardPACK.LogOn()
            {
                UserName = this.textBox1.Text
            };

            Client.SendDataToServer(BufferFormat.FormatFCA(tmp));
        }
    }
}
