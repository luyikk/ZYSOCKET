using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.share;
using BuffLibrary;
using ZYSocket.Security;
namespace Client
{
    /// <summary>
    /// 登入界面
    /// </summary>
    public partial class LogOn : Form
    {
        /// <summary>
        /// 登入成功为TRUE
        /// </summary>
        public bool Logins { get; set; }
        byte[] keys = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public LogOn()
        {
            //注册数据包输入事件
            SocketManager.DataInput += new ZYSocket.ClientA.DataOn(SocketManager_DataInput);
            InitializeComponent();
            //测试关闭事件
            this.Closed += new EventHandler(LogOn_Closed);
            Logins = false;
        }

        void LogOn_Closed(object sender, EventArgs e)
        {
            //删除数据包输入事件
            SocketManager.DataInput -= new ZYSocket.ClientA.DataOn(SocketManager_DataInput);
        }

        /// <summary>
        /// 输入包输入处理
        /// </summary>
        /// <param name="Data"></param>
        void SocketManager_DataInput(byte[] Data)
        {
            ReadBytes read = new ReadBytes(Data, 4, -1, (pdata) => DES.DecryptDES(pdata, keys, "------TEST------"));

            int length;
            int cmd;

            if (read.IsDataExtraSuccess && read.ReadInt32(out length) && read.ReadInt32(out cmd) && length == read.Length)
            {
                switch (cmd)
                {
                    case 500: //如果是Message类型数据包
                        BuffLibrary.Message ms;
                        if (read.ReadObject<BuffLibrary.Message>(out ms))
                        {

                            if (ms != null)
                            {
                                if (ms.Type == 1) //1登入失败
                                    MessageBox.Show(ms.MessageStr);
                                else if (ms.Type == 2) //2登入成功
                                {
                                    Logins = true; //设置 LOGINS
                                    this.BeginInvoke(new EventHandler((o, p) => Close()));  //关闭窗口
                                   
                                }
                            }
                        }

                        break;
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login temp = new Login() //发送一个登入数据包
            {
                UserName = this.textBox1.Text,
                PassWord = this.textBox2.Text
            };

            SocketManager.client.SendTo(BufferFormat.FormatFCA(temp,(pdata)=>DES.EncryptDES(pdata,keys,"------TEST------")));
        }
    }
}
