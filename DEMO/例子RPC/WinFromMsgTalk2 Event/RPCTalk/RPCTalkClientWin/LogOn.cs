using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RPCTalkClientWin
{
    public partial class LogOn : Form
    {
        public bool IsLogOn { get; set; }


        public LogOn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsLogOn= TalkService.Client.GetRPC<TalkServer>().LogOn(this.textBox1.Text, this.textBox2.Text);

            if(!IsLogOn)
            {
                MessageBox.Show("用户名密码错误");
            }else
            {
                this.Close();
            }
            
        }
    }
}
