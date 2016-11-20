using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using ZYSocket.share;
using ZYSocket.Compression;
using PackHandler;

namespace Client
{
    public partial class LogOn : Form
    {
        public bool IsLogOn { get; set; }


        public LogOn()
        {
            InitializeComponent();

           
        }

        void SocketManager_BinaryInput(byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data, Deflate.Decompress);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                PackType cmdtype = (PackType)cmd;

                switch (cmdtype)
                {
                    case PackType.LogOnRes:
                        {
                            LogOnRes res;
                            if (read.ReadObject<LogOnRes>(out res))
                            {
                                if (res.IsOk)
                                {
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            IsLogOn = true;
                                            SocketManager.BinaryInput -= new ZYSocket.ClientB.ClientBinaryInputHandler(SocketManager_BinaryInput); //注意这里须要取消事件，切勿遗忘
                                            SocketManager.Disconnet -= new ZYSocket.ClientB.ClientMessageInputHandler(SocketManager_Disconnet);
                                            Close();
                                        }));
                                }
                                else
                                {
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        MessageBox.Show(res.Msg);
                                       
                                    }));
                                }
                            }

                        }
                        break;

                }

            }
        }

        private void LogOn_Load(object sender, EventArgs e)
        {
            SocketManager.BinaryInput += new ZYSocket.ClientB.ClientBinaryInputHandler(SocketManager_BinaryInput);
            SocketManager.Disconnet += new ZYSocket.ClientB.ClientMessageInputHandler(SocketManager_Disconnet);
        }

        void SocketManager_Disconnet(string message)
        {           
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                MessageBox.Show(message);
                Close();
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ip = System.Configuration.ConfigurationManager.AppSettings["Host"];
            int port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Port"]);

            if (!SocketManager.IsConnent)
            {
                if (!SocketManager.Connent(ip, port))
                {
                    MessageBox.Show("无法连接服务器");
                    //Close();
                }
                else
                {
                    SocketManager.StartRead();
                }
            }

            PackHandler.LogOn logon = new PackHandler.LogOn()
            {
                UserName = this.textBox1.Text,
                PassWord = this.textBox2.Text
            };

            SocketManager.Send(BufferFormatV2.FormatFCA(logon, Deflate.Compress));



        }


    }
}
