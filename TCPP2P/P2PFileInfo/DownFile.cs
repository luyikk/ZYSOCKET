using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using P2PFileInfo.Pack;
using ZYSocket.share;
using System.IO;

namespace P2PFileInfo
{
    public partial class DownFile : Form
    {
        UserInfo user;
        Down down;
        public List<CheckB> IsCheckTable { get; set; }
        public FileStream FStream { get; set; }

        public bool IsClose { get; set; }
        public int ProcessValue { get; set; }
        public int ProcessMax { get; set; }
        public long SizeR { get; set; }
        public string DownDir { get; set; }
        public static int lengt = 4096;


        public DownFile(UserInfo user,Down down)
        {
            InitializeComponent();
            user.DownDataOn = DataIn;
            this.user = user;
            this.down = down;
            this.FormClosed += new FormClosedEventHandler(DownFile_FormClosed);
            IsCheckTable = new List<CheckB>();
            this.Text = "正在下载:" + down.FullName;
        }

        void DownFile_FormClosed(object sender, FormClosedEventArgs e)
        {
            BufferFormatV2 buff = new BufferFormatV2((int)FileCmd.DownClose);
            buff.AddItem(down.DownKey);
            user.SendData(buff.Finish());

            if (FStream != null)
                FStream.Close();

            user.DownDataOn = null;
        }

        public void DataIn(byte[] data)
        {

            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 2001:
                        {
                            long Key;
                            if (read.ReadInt64(out Key))
                            {
                                if (Key == down.DownKey)
                                {
                                    string msg;
                                    if (read.ReadString(out msg))
                                    {
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            MessageBox.Show(msg);
                                            this.Close();
                                        }));
                                    }
                                }

                            }
                        }
                        break;
                    case 2002:
                        {
                            long Key;
                            if (read.ReadInt64(out Key))
                            {
                                if (Key == down.DownKey)
                                {
                                    long startp;
                                    long endp;
                                    byte[] buff;

                                    if (read.ReadInt64(out startp) && read.ReadInt64(out endp) && read.ReadByteArray(out buff))
                                    {
                                        System.Threading.ThreadPool.QueueUserWorkItem((a) =>
                                        {
                                            CheckB cb = IsCheckTable.Find(p => p.StartPostion == startp);

                                            if (cb != null)
                                            {
                                                if (cb.EndPostion == endp && buff.Length >= cb.Size)
                                                {
                                                    cb.Checkd = true;

                                                    FStream.Position = cb.StartPostion;
                                                    FStream.Write(buff, 0, cb.Size);
                                                    SizeR += cb.Size;


                                                    this.BeginInvoke(new EventHandler((a1, b1) =>
                                                    {
                                                        ProcessValue++;
                                                        if (ProcessValue <= this.progressBar1.Maximum)
                                                            this.progressBar1.Value = ProcessValue;
                                                        else
                                                            this.progressBar1.Value = this.progressBar1.Maximum;

                                                        this.label1.Text = Math.Round(((double)SizeR / 1024 / 1024), 2) + "MB /" + Math.Round((double)down.Size / 1024 / 1024, 2) + "MB";
                                                    }));

                                                }
                                            }
                                            else
                                            {
                                                this.BeginInvoke(new EventHandler((a1, b1) =>
                                                {
                                                    BufferFormatV2 bufff = new BufferFormatV2((int)FileCmd.DownClose);
                                                    bufff.AddItem(down.DownKey);
                                                    user.SendData(bufff.Finish());
                                                    MessageBox.Show("数据验证出错??");
                                                    Close();
                                                }));
                                            }

                                        });
                                    }
                                }

                            }
                        }
                        break;
                    case 2003:
                        {
                            long Key;
                            if (read.ReadInt64(out Key))
                            {
                                if (Key == down.DownKey)
                                {
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        CheckDown();
                                    }));
                                }

                            }


                        }
                        break;
                    case 2004:
                        {
                            long Key;
                            if (read.ReadInt64(out Key))
                            {
                                if (Key == down.DownKey)
                                {
                                    long startP;
                                    byte[] xdata;

                                    if (read.ReadInt64(out startP) && read.ReadByteArray(out xdata))
                                    {
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            CheckB cb = IsCheckTable.Find(p => p.StartPostion == startP);

                                            if (xdata.Length >= cb.Size)
                                            {
                                                cb.Checkd = true;

                                                FStream.Position = cb.StartPostion;
                                                FStream.Write(xdata, 0, cb.Size);
                                                SizeR += cb.Size;


                                                this.BeginInvoke(new EventHandler((a1, b1) =>
                                                {
                                                    ProcessValue++;
                                                    if (ProcessValue <= this.progressBar1.Maximum)
                                                        this.progressBar1.Value = ProcessValue;
                                                    else
                                                        this.progressBar1.Value = this.progressBar1.Maximum;

                                                    this.label1.Text = Math.Round(((double)SizeR / 1024 / 1024), 2) + "MB /" + Math.Round((double)down.Size / 1024 / 1024, 2) + "MB";
                                                }));

                                            }


                                            CheckDown();
                                        }));
                                    }
                                }

                            }

                        }
                        break;

                }

            }
        }
        private void CheckDown()
        {
            CheckB p = IsCheckTable.Find(x => x.Checkd == false);

            if (p == null)
            {
                BufferFormatV2 buff = new BufferFormatV2((int)FileCmd.DownClose);
                buff.AddItem(down.DownKey);
                user.SendData(buff.Finish());

                FStream.Close();



                this.BeginInvoke(new EventHandler((a, b) =>
                {
                    this.Text = DownDir + "--下载完毕";
                    this.button1.Text = "完成";
                    MessageBox.Show(Text);

                }));
            }
            else
            {
                BufferFormatV2 buff = new BufferFormatV2((int)FileCmd.ReBytes);
                buff.AddItem(down.DownKey);
                buff.AddItem(p.StartPostion);
                buff.AddItem(p.Size);
                user.SendData(buff.Finish());

            }
        }

     
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void LoadingCheckTable()
        {
            long i = 0;


            int plengt = lengt - 1;
            while (true)
            {
                if (i < down.Size)
                {
                    if ((i + plengt) < down.Size)
                    {
                        IsCheckTable.Add(new CheckB()
                        {
                            Size = lengt,
                            StartPostion = i,
                            EndPostion = i + plengt
                        });
                    }
                    else if ((i + plengt) == down.Size)
                    {
                        IsCheckTable.Add(new CheckB()
                        {
                            Size = lengt,
                            StartPostion = i,
                            EndPostion = i + plengt
                        });

                        break;
                    }
                    else
                    {
                        long t = down.Size - i;


                        IsCheckTable.Add(new CheckB()
                        {
                            Size = (int)t,
                            StartPostion = i,
                            EndPostion = i + t - 1
                        });

                        break;

                    }
                }
                else
                {
                    break;
                }

                i += lengt;

            }
        }

        private void DownFile_Load(object sender, EventArgs e)
        {
            LoadingCheckTable();

            ProcessMax = IsCheckTable.Count;

            this.progressBar1.Maximum = ProcessMax;
            this.progressBar1.Value = ProcessValue;

            FileInfo test = new FileInfo(down.FullName);

            saveFileDialog1.FileName = test.Name;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.ReadWrite);
                DownDir = saveFileDialog1.FileName;
                if (down.Size == 0)
                {
                    this.Text = DownDir + "--下载完毕";
                    this.button1.Text = "完成";
                    MessageBox.Show(Text);

                    Close();
                    return;
                }

                BufferFormatV2 buff = new BufferFormatV2((int)FileCmd.DownNow);
                buff.AddItem(down.DownKey);
                user.SendData(buff.Finish());
            }
            else
            {
                this.Close();
            }
        }
    }
}
