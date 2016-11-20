using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileManager
{
    public partial class AddUser : Form
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Path { get; set; }

        public bool IsAdd { get; set; }

        public AddUser()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                this.textBox3.Text = this.folderBrowserDialog1.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            UserName = this.textBox1.Text;
            PassWord = this.textBox2.Text;
            Path = this.textBox3.Text;

            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(PassWord) && !string.IsNullOrEmpty(Path))
            {
                IsAdd = true;
                this.Close();
            }
            else
                MessageBox.Show("请检查输入");
        }
    }
}
