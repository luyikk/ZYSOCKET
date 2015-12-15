using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace P2PFileInfo
{
    public partial class LogOn : Form
    {
        public string PassWrod { get; set; }

        public LogOn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PassWrod = this.textBox1.Text;
        }
    }
}
