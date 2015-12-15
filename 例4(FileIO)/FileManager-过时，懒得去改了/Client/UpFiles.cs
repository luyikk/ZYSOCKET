using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using ZYSocket.share;
using ZYSocket.Compression;
using PackHandler;
namespace Client
{
    public partial class UpFiles : Form
    {
        public long Key { get; set; }
        public FileStream Stream { get; set; }
        public bool IsClose { get; set; }
        public static int bytelengt = 16384;
        private long fileLength { get; set; }
        private long sendlength { get; set; }
      
        private WinMain main;
        public UpFiles(WinMain main,string filename, long key,FileStream stream)
        {
            InitializeComponent();
            this.Key=key;
            this.Stream = stream;        
            this.main = main;
            main.UpFileClose += new UpFileCloseHandler(main_UpFileClose);
            this.FormClosed += new FormClosedEventHandler(UpFiles_FormClosed);
            fileLength = stream.Length;
          
            progressBar1.Maximum = 1000;
            this.progressBar1.Value = 0;

            this.Text = filename;

            button2.Enabled = false;
        }

        void UpFiles_FormClosed(object sender, FormClosedEventArgs e)
        {
            BufferFormatV2 buff = new BufferFormatV2((int)PackType.UpClose);
            buff.AddItem(Key);
            SocketManager.Send(buff.Finish());

            main.UpFileClose -= new UpFileCloseHandler(main_UpFileClose);
        }

        void main_UpFileClose(long key)
        {
            if (key == Key)
            {
                IsClose = true;
                this.Text = "上传完毕";
                this.button1.Text = "完成";
                this.Text += "----完成";
            }
        }

        private void UpFiles_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(UpFileThread));
        }

        private void UpFileThread(object o)
        {
            try
            {
                byte[] data = new byte[bytelengt];

                int r = 0;
                long p = 0;
                do
                {
                    if (IsClose)
                        break;

                    r = Stream.Read(data, 0, data.Length);

                    if (r < data.Length && r > 0)
                    {
                        byte[] buffb = new byte[r];

                        Buffer.BlockCopy(data, 0, buffb, 0, buffb.Length);
                        BufferFormatV2 buff = new BufferFormatV2((int)PackType.DateUp);
                        buff.AddItem(Key);
                        buff.AddItem(p);
                        buff.AddItem(p + r - 1);
                        buff.AddItem(buffb);

                        SocketManager.Send(buff.Finish());

                        sendlength += r;

                        this.BeginInvoke(new EventHandler((a, b) =>
                        {
                            //if( this.progressBar1.Value< this.progressBar1.Maximum)
                            //    this.progressBar1.Value++;
                            this.label1.Text = Math.Round(((double)sendlength / 1024 / 1024), 4) + "MB/" + Math.Round(((double)fileLength / 1024 / 1024), 4) + "MB";

                            double x = ((double)sendlength / fileLength * 1000);

                            this.progressBar1.Value = (int)x;
                        }));

                        break;
                    }
                    else if (r > 0)
                    {
                        BufferFormatV2 buff = new BufferFormatV2((int)PackType.DateUp);
                        buff.AddItem(Key);
                        buff.AddItem(p);
                        buff.AddItem(p + r - 1);
                        buff.AddItem(data);
                        SocketManager.Send(buff.Finish());

                        p += r;
                    }

                    sendlength += r;

                    this.BeginInvoke(new EventHandler((a, b) =>
                        {

                            this.label1.Text = Math.Round(((double)sendlength / 1024 / 1024), 4) + "MB/" + Math.Round(((double)fileLength / 1024 / 1024), 4) + "MB";

                            double x =((double)sendlength / fileLength * 1000);

                            this.progressBar1.Value = (int)x;
                        }));


                } while (r > 0);



                BufferFormatV2 buffcheck = new BufferFormatV2((int)PackType.UpCheck);
                buffcheck.AddItem(Key);
                SocketManager.Send(buffcheck.Finish());

                this.BeginInvoke(new EventHandler((a, b) =>
                    {
                        button2.Enabled = true;
                    }));

            }
            catch (System.Net.Sockets.SocketException e)
            {
                MessageBox.Show("上传文件发生错误:" + e.Message);
                IsClose = true;
            }
            catch (Exception er)
            {
                BufferFormatV2 buff = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                buff.AddItem(Key);
                SocketManager.Send(buff.Finish());
                IsClose = true;
                MessageBox.Show("上传文件发生错误:" + er.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BufferFormatV2 buffcheck = new BufferFormatV2((int)PackType.UpCheck);
            buffcheck.AddItem(Key);
            SocketManager.Send(buffcheck.Finish());
        }
    }
}
