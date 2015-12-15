using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.RPC.Client;
using RPCTest;

namespace RPCTestClient
{
    public partial class WinMain : Form
    {
        public RPCClient client { get; set; }

        public WinMain()
        {
            InitializeComponent();
        }

        private void WinMain_Load(object sender, EventArgs e)
        {
            client = new RPCClient();

            if (client.Connection("127.0.0.1", 998))
            {  
                LogOn tmp = new LogOn();

                tmp.ShowDialog();

                if (tmp.LogOnName != null)
                {

                    string msg = null;
                    if (client.Call<TestUserLogOn, bool>(p => p.IsLogON(tmp.LogOnName, out msg)))
                    {
                        this.Text = msg;
                    }
                    else
                    {
                        MessageBox.Show("登入失败");
                        this.Close();
                    }

                }
            }
            else
            {
                MessageBox.Show("无法连接服务器");
                this.Close();
            }
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            List<UserData> datalist = client.Call<TestUserLogOn, List<UserData>>(p => p.GetData());

            this.dataGridView1.DataSource = datalist;
         
        }

        private void button2_Click(object sender, EventArgs e)
        {

            client.Call<ITest>(p => p.Run("测试开始"));


            client.Call<ITest>(p => p.Run("时间:" + DateTime.Now)); //单体调用


            int i = 0;

             i = client.Call<ITest, int>(p => p.GetRes(i));
            Console.WriteLine("返回整数:" + i);



            List<string> atestlist = new List<string>();

            for (int j = 0; j < 10; j++)
            {
                atestlist.Add(DateTime.Now.ToString());
            }

            List<string> testnewlist = client.Call<ITest, List<string>>(p => p.GetResOP(atestlist));

            Console.WriteLine("获取List对象:" + testnewlist.Count);


            DateTime time = client.Call<TestTow, DateTime>(p => p.GetTime());

            Console.WriteLine("服务器当前时间:" + time);


            client.Call<TestTow>(p => p.SetR(1000));


            Console.WriteLine("GET R:" + client.Call<TestTow, int>(p => p.GetR()));


            //-----------测试引用------

            string a = "1";          
            bool b = client.Call<TestTow, bool>(p => p.TestRef(ref a));

            Console.WriteLine("ref :" + a);

            //----------测试 out-------



            client.Call<TestTow>(p => p.TestOut("",out a));

            Console.WriteLine("out :" + a);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.label1.Text = client.Call<TestUserLogOn, string>(p => p.GetMyIP());
        }
    }
}
