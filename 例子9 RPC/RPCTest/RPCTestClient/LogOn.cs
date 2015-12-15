using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RPCTestClient
{
    public partial class LogOn : Form
    {
        public string LogOnName { get; set; }

        public LogOn()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.LogOnName = this.textBox1.Text;

            this.Close();
        }
    }
}
