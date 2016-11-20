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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ZYNetClient Client { get; set; }



        public Graphics gs  { get; set; }
        public Graphics gs2 { get; set; }
        public bool IsDown { get; set; }

        public SolidBrush Br { get; set; }

        public Bitmap map { get; set; }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            IsDown = true;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            IsDown = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDown)
            {
                gs.FillEllipse(Br, e.X, e.Y, 2, 2);
                gs.Flush();
                gs.Save();

                gs2.FillEllipse(Br, e.X, e.Y, 2, 2);
                gs2.Flush();
                gs2.Save();

                DrawPoint tmp = new DrawPoint()
                {
                    X = e.X,
                    Y = e.Y,
                    Color = Br.Color.ToArgb()
                };

                Client.SendDataToALL(BufferFormat.FormatFCA(tmp));

            }

        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            map = new Bitmap(this.panel1.Width, this.panel1.Height);        

            gs = Graphics.FromImage(map);
            gs2 = this.panel1.CreateGraphics();

            Br = new SolidBrush(Color.FromArgb(new Random().Next()));

            Client = new ZYNetClient();
            if (Client.Connect(ZYSocket.share.RConfig.ReadString("host"), ZYSocket.share.RConfig.ReadInt("ServicePort")))
            {
                LogOn tmp = new LogOn(Client);
                tmp.ShowDialog();

                if (tmp.IsLogOn)
                {
                    Client.ServerDisconnect += Client_ServerDisconnect;
                    Client.DataInput += Client_DataInput;

                    UserCount count = new UserCount();
                    Client.SendDataToServer(BufferFormat.FormatFCA(count));

                    this.Text = tmp.Names;
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("无法连接服务器");
                this.Close();
            }        
            

        }

        private void Client_ServerDisconnect(string message)
        {
            MessageBox.Show(message);
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                this.Close();
            }));
            
        }

        private void Client_DataInput(long Id, byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int lengt;
            int cmd;
            if(read.ReadInt32(out lengt)&&read.ReadInt32(out cmd)&&lengt==read.Length)
            {
                switch(cmd)
                {
                    case 2000:
                        {
                            DrawPoint tmp;

                            if(read.ReadObject<DrawPoint>(out tmp))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                {
                                    Brush br = new SolidBrush(Color.FromArgb(tmp.Color));

                                    gs.FillEllipse(br, tmp.X, tmp.Y, 2, 2);
                                    gs.Flush();
                                    gs.Save();

                                    gs2.FillEllipse(br, tmp.X, tmp.Y, 2, 2);
                                    gs2.Flush();
                                    gs2.Save();

                                }));
                            }

                        }
                        break;
                    case 3000:
                        {
                            this.BeginInvoke(new EventHandler((a, b) =>
                            {
                                gs.Clear(Color.White);
                                gs2.Clear(Color.White);
                            }));
                        }
                        break;
                    case 5000:
                        {
                            if (Id != 0) //id=0表示服务器 其他没有特殊需要别管
                                return;

                            UserCount tmp;
                            if(read.ReadObject<UserCount>(out tmp))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                {
                                    this.label1.Text = "当前人数:"+tmp.Count;

                                }));
                            }

                        }
                        break;
                    

                }


            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if( this.colorDialog1.ShowDialog()==DialogResult.OK)
            {
                Br = new SolidBrush(colorDialog1.Color);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            gs.Clear(Color.White);
            gs2.Clear(Color.White);

            ClecrColor tmp = new ClecrColor();
            tmp.Color = Color.White.ToArgb();
            Client.SendDataToALL(BufferFormat.FormatFCA(tmp));
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            gs2.DrawImage(map, 0, 0);
            gs2.Flush();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveImg tmp = new SaveImg()
            {
               FileName=Guid.NewGuid().ToString().Replace("-","")+".bmp"
            };

            Client.SendDataToALL(BufferFormat.FormatFCA(tmp));
        }
    }
}
