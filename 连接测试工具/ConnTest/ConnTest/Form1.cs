using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace ConnTest
{
    public partial class Form1 : Form
    {

        List<Client> ClientList;


        float R = 10.0f;

        byte[] data;

        public Form1()
        {
            InitializeComponent();
            ClientList = new List<Client>();
            ZYSocket.share.BufferFormat formt = new ZYSocket.share.BufferFormat(1003);
            formt.AddItem(DateTime.Now);
            data = formt.Finish();
           
           
        }

      

        private void button1_Click(object sender, EventArgs e)
        {

            this.R =(float)this.numericUpDown2.Value;

           

            Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < this.numericUpDown1.Value; i++)
                    {
                        Client temp = new Client(this.textBox1.Text, int.Parse(this.textBox2.Text));
                        temp.Connto();
                        this.ClientList.Add(temp);
                        this.BeginInvoke(new EventHandler((a,b)=>
                        {
                            this.label6.Text=this.ClientList.Count.ToString();
                        })).AsyncWaitHandle.WaitOne(10);

                        System.Threading.Thread.Sleep(2);
                    }
                });
        }

        private void button2_Click(object sender, EventArgs er)
        {

            Task.Factory.StartNew(() =>
                {


                    int maxX = this.panel1.Width;
                    int maxY = this.panel1.Height;

                    int maxXCout = (int)(maxX / R);
                    int maxYCout = (int)(maxY / R);

                    this.BeginInvoke(new EventHandler((ob, ca) =>
                        {

                            for (int p = 0; p < ClientList.Count; p++)
                            {
                                Client client = ClientList[p];


                                float drawX = p % maxXCout * R;
                                float drawY = p / maxXCout * R;

                                if (client != null)
                                {
                                    using (Graphics e = Graphics.FromHwnd(this.panel1.Handle))
                                    {
                                        if (client.IsConn && client.IsRead)
                                        {
                                            e.FillEllipse(Brushes.Green, drawX, drawY, R, R);
                                        }
                                        else if (client.IsConn)
                                        {
                                            e.FillEllipse(Brushes.Blue, drawX, drawY, R, R);
                                        }
                                        else if (!client.IsConn)
                                        {
                                            e.FillEllipse(Brushes.Red, drawX, drawY, R, R);
                                        }
                                        else
                                        {
                                            e.FillEllipse(Brushes.Black, drawX, drawY, R, R);
                                        }
                                        e.Save();
                                    }

                                }
                            }                                
                            
                        }));
                });
        }
                

        private void button3_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach<Client>(ClientList, c => c.SendData(data));
                    MessageBox.Show("发送完毕");
                });
        }
     
       
    }
}
