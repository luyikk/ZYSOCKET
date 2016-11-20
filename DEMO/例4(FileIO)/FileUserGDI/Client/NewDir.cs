using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class NewDir : Form
    {
        public string DirName { get; set; }

        public NewDir()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DirName = this.textBox1.Text;
            this.Close();
        }
    }
}
