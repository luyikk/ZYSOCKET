namespace Client
{
    partial class WinMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinMain));
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._Down = new System.Windows.Forms.ToolStripMenuItem();
            this._Up = new System.Windows.Forms.ToolStripMenuItem();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._Run = new System.Windows.Forms.ToolStripMenuItem();
            this._Move1 = new System.Windows.Forms.ToolStripMenuItem();
            this._Move2 = new System.Windows.Forms.ToolStripMenuItem();
            this._Del = new System.Windows.Forms.ToolStripMenuItem();
            this._ReName = new System.Windows.Forms.ToolStripMenuItem();
            this._CreateDir = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.AllowDrop = true;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.LabelEdit = true;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(8, 58);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(613, 403);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            this.listView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_AfterLabelEdit);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView1_DragDrop);
            this.listView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView1_DragEnter);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._Down,
            this._Up,
            this.刷新ToolStripMenuItem,
            this._Run,
            this._Move1,
            this._Move2,
            this._Del,
            this._ReName,
            this._CreateDir});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 202);
            // 
            // _Down
            // 
            this._Down.Name = "_Down";
            this._Down.Size = new System.Drawing.Size(136, 22);
            this._Down.Text = "下载";
            this._Down.Click += new System.EventHandler(this._Down_Click);
            // 
            // _Up
            // 
            this._Up.Name = "_Up";
            this._Up.Size = new System.Drawing.Size(136, 22);
            this._Up.Text = "上传";
            this._Up.Click += new System.EventHandler(this._Up_Click);
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.刷新ToolStripMenuItem.Text = "刷新";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.刷新ToolStripMenuItem_Click);
            // 
            // _Run
            // 
            this._Run.Name = "_Run";
            this._Run.Size = new System.Drawing.Size(136, 22);
            this._Run.Text = "服务器运行";
            this._Run.Click += new System.EventHandler(this._Run_Click);
            // 
            // _Move1
            // 
            this._Move1.Name = "_Move1";
            this._Move1.Size = new System.Drawing.Size(136, 22);
            this._Move1.Text = "剪贴";
            this._Move1.Click += new System.EventHandler(this._Move1_Click);
            // 
            // _Move2
            // 
            this._Move2.Name = "_Move2";
            this._Move2.Size = new System.Drawing.Size(136, 22);
            this._Move2.Text = "粘贴";
            this._Move2.Click += new System.EventHandler(this._Move2_Click);
            // 
            // _Del
            // 
            this._Del.Name = "_Del";
            this._Del.Size = new System.Drawing.Size(136, 22);
            this._Del.Text = "删除";
            this._Del.Click += new System.EventHandler(this._Del_Click);
            // 
            // _ReName
            // 
            this._ReName.Name = "_ReName";
            this._ReName.Size = new System.Drawing.Size(136, 22);
            this._ReName.Text = "重命名文件";
            this._ReName.Click += new System.EventHandler(this._ReName_Click);
            // 
            // _CreateDir
            // 
            this._CreateDir.Name = "_CreateDir";
            this._CreateDir.Size = new System.Drawing.Size(136, 22);
            this._CreateDir.Text = "新建文件夹";
            this._CreateDir.Click += new System.EventHandler(this._CreateDir_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Disk.jpg");
            this.imageList1.Images.SetKeyName(1, "Dir.jpg");
            this.imageList1.Images.SetKeyName(2, "CDROOM.jpg");
            this.imageList1.Images.SetKeyName(3, "Dll.jpg");
            this.imageList1.Images.SetKeyName(4, "exe.jpg");
            this.imageList1.Images.SetKeyName(5, "File.jpg");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(464, 31);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "上层目录";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "路径:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(51, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(519, 21);
            this.textBox1.TabIndex = 3;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(576, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(45, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "跳转";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 468);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 6;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "*.*|*.*";
            this.openFileDialog1.Title = "请选择上传的文件";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "双击打开";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // WinMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 489);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WinMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FileManager-Client UserInfo ";
            this.Load += new System.EventHandler(this.WinMain_Load);
            this.SizeChanged += new System.EventHandler(this.WinMain_SizeChanged);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem _Down;
        private System.Windows.Forms.ToolStripMenuItem _Up;
        private System.Windows.Forms.ToolStripMenuItem _Run;
        private System.Windows.Forms.ToolStripMenuItem _Move1;
        private System.Windows.Forms.ToolStripMenuItem _Move2;
        private System.Windows.Forms.ToolStripMenuItem _Del;
        private System.Windows.Forms.ToolStripMenuItem _ReName;
        private System.Windows.Forms.ToolStripMenuItem _CreateDir;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

