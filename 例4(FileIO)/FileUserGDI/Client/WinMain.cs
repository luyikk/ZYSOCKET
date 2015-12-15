using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZYSocket.Compression;
using ZYSocket.share;
using PackHandler;
using System.IO;
namespace Client
{
    internal delegate void UpFileCloseHandler(long key);
    public partial class WinMain : Form
    {
    

        public FileSystem MoveFile { get; set; }
        public string CurrentDir { get; set; }

        public Dictionary<long, FileStream> UpFileList { get; set; }

        internal event UpFileCloseHandler UpFileClose;

        public string Path { get; set; }
        public WinMain()
        {
            InitializeComponent();
            UpFileList = new Dictionary<long, FileStream>();
        }

        private void WinMain_Load(object sender, EventArgs e)
        {
           
            BufferFormatV2.ObjFormatType = BuffFormatType.XML;
            ReadBytesV2.ObjFormatType = BuffFormatType.XML;
            

            LogOn logon = new LogOn();
            logon.ShowDialog();

            if (!logon.IsLogOn)
            {
                Close();
                return;
            }

            Path = logon.Path;

            SocketManager.BinaryInput += new ZYSocket.ClientB.ClientBinaryInputHandler(SocketManager_BinaryInput);
            SocketManager.Disconnet += new ZYSocket.ClientB.ClientMessageInputHandler(SocketManager_Disconnet);
                     
            
            LoadingDiskInfo();            

            this.WindowState = FormWindowState.Minimized;
         

        }



        void SocketManager_Disconnet(string message)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                MessageBox.Show(message);
                Close();
            }));
        }

        void SocketManager_BinaryInput(byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data, Deflate.Decompress);
            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                PackType cmdtype = (PackType)cmd;

                switch (cmdtype)
                {
                    case PackType.GetDisk:                       
                    case PackType.Dir:
                        {

                               Dir dir;
                               if (read.ReadObject<Dir>(out dir))
                               {
                                 

                                   this.BeginInvoke(new EventHandler((a, b) =>
                                       {
                                           if (dir.IsSuccess)
                                           {
                                               this.button1.Enabled = true;
                                               this.button1.Text = "上层目录";

                                               this.listView1.Clear();

                                               dir.FileSystemList.Sort(new Comparison<FileSystem>((a1, a2) =>
                                                   {
                                                       if (a1.FileType == FileType.Dir && a2.FileType == FileType.Dir)
                                                       {
                                                           return a1.Name.CompareTo(a2.Name);

                                                       }
                                                       else if(a1.FileType != FileType.Dir && a2.FileType != FileType.Dir)
                                                       {
                                                           return a1.Name.CompareTo(a2.Name);
                                                       }
                                                       else if (a1.FileType == FileType.Dir && a2.FileType == FileType.File)
                                                       {
                                                           return -1;
                                                       }
                                                       else if (a2.FileType == FileType.Dir && a1.FileType == FileType.File)
                                                       {
                                                           return 1;
                                                       }
                                                       else
                                                       {
                                                           return 0;
                                                       }

                                                   }));


                                               SystemICO.ListViewSysImages(this.listView1);
                                               int x1=0,x2=0;
                                               foreach (FileSystem p in dir.FileSystemList)
                                               {
                                                   if (p.FileType == FileType.File)
                                                   {

                                                       this.listView1.Items.Add(new ListViewItem()
                                                       {
                                                           Text = p.Name,
                                                           ImageIndex = GetImageIndex(p.Name),
                                                           Tag=p

                                                       });
                                                       x1++;
                                                   }
                                                   else
                                                   {
                                                       this.listView1.Items.Add(new ListViewItem()
                                                       {
                                                           Text = p.Name,
                                                           ImageIndex = 1,
                                                           Tag = p
                                                       });

                                                       x2++;
                                                   }

                                               }
                                               
                                               if (!string.IsNullOrEmpty(Path))
                                               {
                                                   string apath = dir.DirName;

                                                   apath=apath.Remove(0, Path.Length);

                                                   if (string.IsNullOrEmpty(apath))
                                                       apath = "\\";

                                                   this.textBox1.Text = apath;

                                               }else
                                                    this.textBox1.Text = dir.DirName;

                                               CurrentDir = dir.DirName;

                                               this.label2.Text = "共找到目录:" + x2 + "个，文件:" + x1 + "个";
                                           }
                                           else
                                           {
                                               MessageBox.Show(dir.Msg);
                                           }
                                       }));
                               }

                        }
                        break;
                    case PackType.DelFile:
                        {
                             DelFile dfile;
                             if (read.ReadObject<DelFile>(out dfile))
                             {
                                 this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        string isErr = "";
                                        foreach (DelFileName p in dfile.DelFileList)
                                        {
                                            if (!p.IsSuccess)
                                            {
                                                isErr += p.Msg + "\r\n";
                                            }

                                        }

                                        if (isErr == "")
                                        {

                                            MessageBox.Show("删除成功");

                                        }
                                        else
                                        {
                                            MessageBox.Show(isErr);
                                        }

                                        GotoDir();

                                    }));
                             }
                        }
                        break;
                    case PackType.NewDir:
                        {                            
                             PackHandler.NewDir ndir;
                             if (read.ReadObject<PackHandler.NewDir>(out ndir))
                             {
                                 this.BeginInvoke(new EventHandler((a, b) =>
                                       {
                                           if (ndir.IsSuccess)
                                           {
                                               GotoDir();
                                           }
                                           else
                                           {
                                               MessageBox.Show(ndir.Msg);
                                               GotoDir();
                                           }
                                       }));
                             }
                        }
                        break;
                    case PackType.MoveFileSystem:
                        {
                            PackHandler.MoveFileSystem mfs;

                            if (read.ReadObject<PackHandler.MoveFileSystem>(out mfs))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            if (!mfs.IsSuccess)
                                            {
                                                MessageBox.Show(mfs.Msg);
                                                GotoDir();
                                            }

                                            if (this.MoveFile != null)
                                            {
                                                if (mfs.OldName == MoveFile.FullName)
                                                {
                                                   
                                                    GotoDir();

                                                    this.MoveFile = null;
                                                }
                                            }
                                        }));
                            }

                        }
                        break;
                    case PackType.Run:
                        {
                            Run run;
                            if (read.ReadObject<Run>(out run))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                       {
                                           if (run.IsSuccess)
                                           {
                                               MessageBox.Show("运行成功");
                                           }
                                           else
                                           {
                                               MessageBox.Show(run.Msg);
                                           }
                                       }));

                            }
                        }
                        break;
                    case PackType.Down:
                        {
                            Down down;
                            if (read.ReadObject<Down>(out down))
                            {
                                this.BeginInvoke(new EventHandler((a, b) =>
                                         {
                                             if (down.IsSuccess)
                                             {
                                                 DownFile downwin = new DownFile(down);
                                                 downwin.Show();


                                             }
                                             else
                                             {

                                                 MessageBox.Show(down.Msg);

                                             }

                                         }));
                            }

                        }
                        break;
                    case PackType.UpFile:
                        {
                            UpFile upFile;
                            if (read.ReadObject<UpFile>(out upFile))
                            {
                                if (!upFile.IsSuccess)
                                {
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                         {
                                             MessageBox.Show("上传文件发生错误:" + upFile.Msg);
                                         }));
                                }
                                else
                                {
                                    if (UpFileList.ContainsKey(upFile.UpKey))
                                    {
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                      {
                                          FileStream stream = UpFileList[upFile.UpKey];

                                          UpFiles win = new UpFiles(this,upFile.FullName, upFile.UpKey, stream);
                                          win.Show();

                                      }));

                                    }
                                    else
                                    {
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            MessageBox.Show("上传文件发生错误:无法找到KEY所指定的文件");
                                        }));

                                    }

                                }

                            }

                        }
                        break;
                    case PackType.UpClose:
                        {
                            long key;
                            if (read.ReadInt64(out key))
                            {
                                if (UpFileList.ContainsKey(key))
                                {
                                    this.UpFileList[key].Close();
                                    this.UpFileList.Remove(key);
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            if (UpFileClose != null)
                                                UpFileClose(key);
                                        }));
                                }
                            }
                        }
                        break;
                    case PackType.ReBytes:
                        {
                            long downkey;

                            if (read.ReadInt64(out downkey))
                            {
                                long startpostion;
                                int size;

                                if (read.ReadInt64(out startpostion) && read.ReadInt32(out size))
                                {
                                    if (this.UpFileList.ContainsKey(downkey))
                                    {
                                        FileStream strem = UpFileList[downkey];

                                        strem.Position = startpostion;

                                        byte[] xdata = new byte[size];

                                        strem.Read(xdata, 0, xdata.Length);


                                        BufferFormatV2 buff = new BufferFormatV2((int)PackType.DataSet);
                                        buff.AddItem(downkey);
                                        buff.AddItem(startpostion);
                                        buff.AddItem(xdata);                                     
                                        SocketManager.Send(buff.Finish());
                                    }
                                    else
                                    {
                                        BufferFormatV2 buff = new BufferFormatV2((int)PackType.UpClose);
                                        buff.AddItem(downkey);
                                        SocketManager.Send(buff.Finish());
                                    }

                                }
                            }

                        }
                        break;
                }

            }
        }

        private int GetImageIndex(string name)
        {
            string extname = System.IO.Path.GetExtension(name);

            //if (extname.IndexOf(".exe") >= 0)
            //{
            //    return 4;
            //}
            //else if (extname.IndexOf(".dll") >= 0)
            //{
            //    return 3;
            //}
            //else
            //{

           
            string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Icon" + extname);

            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }

            return SystemICO.FileIconIndex(filename);

            //}


        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadingDiskInfo();
        }

        private void LoadingDiskInfo()
        {
            Dir disk = new Dir();
            SocketManager.Send(BufferFormatV2.FormatFCA(disk,Deflate.Compress));
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[listView1.SelectedItems.Count - 1];

                DiskInfo info = item.Tag as DiskInfo;

                if (info != null)
                {
                    ShowDiskInfo(info);
                }
                else
                {
                    FileSystem file = item.Tag as FileSystem;

                    if (file.FileType == FileType.File)
                    {

                        this.label2.Text = "文件:" + file.Name + " 大小:" + Math.Round((double)file.Size / 1024 / 1024, 3) + "MB 最近修改时间:" + file.EditTime;
                    }
                    else
                    {
                        this.label2.Text = "目录:" + file.Name + "最近修改时间:" + file.EditTime;
                    }
                }
            }
        }

        private void ShowDiskInfo(DiskInfo disk)
        {

            String MyType = "";
            switch (disk.DriveType)
            {
                case DriveType.CDRom:
                    MyType = "光盘设备";
                    break;
                case DriveType.Fixed:
                    MyType = "固定硬盘";
                    break;
                case DriveType.Network:
                    MyType = "网络驱动器";
                    break;
                case DriveType.NoRootDirectory:
                    MyType = "没有根目录";
                    break;
                case DriveType.Ram:
                    MyType = "RAM磁盘";
                    break;
                case DriveType.Removable:
                    MyType = "可移动设备";
                    break;
                case DriveType.Unknown:
                    MyType = "未知设备";
                    break;
            }
            if (disk.IsReady)
                this.label2.Text = disk.Name + " 类型:" + MyType + " 总共大小:" + Math.Round((double)disk.TotalSize / 1024 / 1024 / 1024, 2) + "G 可用空闲空间:" + Math.Round((double)disk.TotalFreeSpace / 1024 / 1024 / 1024, 2) + "G";
            else
                this.label2.Text = disk.Name + " 类型:" + MyType + " 状态不可用";
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[listView1.SelectedItems.Count - 1];


                string DirName = "";

                FileSystem dirinfo = item.Tag as FileSystem;

                if (dirinfo != null)
                {
                    if (dirinfo.FileType == FileType.Dir)
                    {
                        DirName = dirinfo.FullName;
                    }
                    else
                    {
                        if (MessageBox.Show("是否下载文件:" + dirinfo.Name + "?","提示",MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            FileSystem downFile = (this.listView1.SelectedItems[this.listView1.SelectedItems.Count - 1].Tag as FileSystem);

                            if (downFile != null)
                            {
                                Down down = new Down()
                                {
                                    FullName = downFile.FullName

                                };

                                SocketManager.Send(BufferFormatV2.FormatFCA(down, Deflate.Compress));

                            }

                        }

                        return;
                    }

                }
                else
                {

                    DiskInfo info = item.Tag as DiskInfo;

                    if (info != null)
                        DirName = info.Name;

                }


                Dir tmp = new Dir()
                {
                    DirName=DirName,
                    FileSystemList = new List<FileSystem>(),
                    IsSuccess = false,
                    Msg = ""
                };

                SocketManager.Send(BufferFormatV2.FormatFCA(tmp,Deflate.Compress));
              
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GotoDir();
        }


      

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GotoDir();
            }
        }

        public void GotoDir()
        {
            try
            {
                DirectoryInfo np = new DirectoryInfo(Path + this.textBox1.Text);
                Dir tmp = new Dir()
                {
                    DirName = np.FullName,
                    FileSystemList = new List<FileSystem>(),
                    IsSuccess = false,
                    Msg = ""
                };

                SocketManager.Send(BufferFormatV2.FormatFCA(tmp, Deflate.Compress));
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(CurrentDir))
            {
                LoadingDiskInfo();
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(CurrentDir);

            if (dir.Parent != null)
            {
                string newdir = dir.Parent.FullName;


                Dir tmp = new Dir()
                {
                    DirName = newdir,
                    FileSystemList = new List<FileSystem>(),
                    IsSuccess = false,
                    Msg = ""
                };

                SocketManager.Send(BufferFormatV2.FormatFCA(tmp, Deflate.Compress));
            }
            else
            {
                LoadingDiskInfo();
            }
        }



        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.listView1.SelectedItems.Count == 0)
                {
                    _Up.Enabled = true;
                    _Down.Enabled = false;
                    _CreateDir.Enabled = true;
                    _Move1.Enabled = false;
                    _Move2.Enabled = true;
                    _Del.Enabled = false;
                    _Run.Enabled = false;
                    _ReName.Enabled = false;


                }
                else if (this.listView1.SelectedItems.Count == 1)
                {
                    _Up.Enabled = false;
                    _Down.Enabled = true;
                    _CreateDir.Enabled = false;
                    _Move1.Enabled = true;
                    _Move2.Enabled = false;
                    _Del.Enabled = true;
                    _Run.Enabled = true;
                    _ReName.Enabled = true;

                    FileSystem fs = this.listView1.SelectedItems[0].Tag as FileSystem;

                    if (fs!=null&&fs.FileType == FileType.Dir)
                    {
                        _Down.Enabled = false;
                        _Run.Enabled = false;
                    }
                }
                else if (this.listView1.SelectedItems.Count > 1)
                {
                    _Up.Enabled = false;
                    _Down.Enabled = false;
                    _CreateDir.Enabled = false;
                    _Move1.Enabled = false;
                    _Move2.Enabled = false;
                    _Del.Enabled = true;
                    _Run.Enabled = false;
                    _ReName.Enabled = true;
                }



                if (this.MoveFile==null)
                {
                    _Move2.Enabled = false;
                }

                

                //if (this.textBox1.Text.IndexOf(":") == -1)
                //{
                //    _Up.Enabled = false;
                //    _Down.Enabled = false;
                //    _CreateDir.Enabled = false;
                //    _Move1.Enabled = false;
                //    _Move2.Enabled = false;
                //    _Del.Enabled = false;
                //    _Run.Enabled = false;
                //    _ReName.Enabled = false;

                //}


            }
        }

        private void _Del_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("你确定要删除吗?删除后将不可恢复", "提示", MessageBoxButtons.YesNo)==DialogResult.Yes)
                {

                    DelFile Dfile = new DelFile();
                    Dfile.DelFileList = new List<DelFileName>();


                    foreach (ListViewItem p in this.listView1.SelectedItems)
                    {
                       FileSystem fs= p.Tag as FileSystem;

                        Dfile.DelFileList.Add(new DelFileName()
                        {
                            FullName=fs.FullName,
                            FType = fs.FileType

                        });

                    }


                    SocketManager.Send(BufferFormatV2.FormatFCA(Dfile, Deflate.Compress));

                }

            }
        }

        private void _CreateDir_Click(object sender, EventArgs e)
        {
            NewDir tmp = new NewDir();
            tmp.ShowDialog();

            if (!string.IsNullOrEmpty(tmp.DirName))
            {
                string fullname = System.IO.Path.Combine(CurrentDir, tmp.DirName);                             
               

                PackHandler.NewDir ndir = new PackHandler.NewDir()
                {
                    DirName = fullname
                };

                

                SocketManager.Send(BufferFormatV2.FormatFCA(ndir, Deflate.Compress));
            }
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = true;
                return;
            }

            FileSystem fs = this.listView1.Items[e.Item].Tag as FileSystem;

            if (fs != null)
            {
                string dir = "";

                if (fs.FileType == FileType.File)
                {
                    dir = (new FileInfo(fs.FullName)).DirectoryName;
                }
                else
                {
                    dir = (new DirectoryInfo(fs.FullName)).Parent.FullName;
                }

                string newname=System.IO.Path.Combine(dir, e.Label);
                string oldname = System.IO.Path.Combine(dir, this.listView1.Items[e.Item].Text);


                fs.FullName = newname;
                fs.Name = e.Label;

                if (oldname != newname)
                {
                    MoveFileSystem(oldname, newname, fs.FileType);
                    e.CancelEdit = false;

                }
                else
                {
                    e.CancelEdit = true;
                }
            }
        }

        private void MoveFileSystem(string oldName, string newName,FileType filetype)
        {
            PackHandler.MoveFileSystem mfs = new MoveFileSystem();
            mfs.OldName = oldName;
            mfs.NewName = newName;
            mfs.FileType = filetype;

            SocketManager.Send(BufferFormatV2.FormatFCA(mfs, Deflate.Compress));

        }

        private void _ReName_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
                this.listView1.SelectedItems[this.listView1.SelectedItems.Count - 1].BeginEdit();
        }

        private void _Move1_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
                this.MoveFile = (this.listView1.SelectedItems[this.listView1.SelectedItems.Count - 1].Tag as FileSystem);
        }

        private void _Move2_Click(object sender, EventArgs e)
        {
            string fullname = System.IO.Path.Combine(CurrentDir, MoveFile.Name);

            MoveFileSystem(MoveFile.FullName, fullname, MoveFile.FileType);


        }

        private void _Run_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                FileSystem fs = (this.listView1.SelectedItems[this.listView1.SelectedItems.Count - 1].Tag as FileSystem);

                if (fs != null)
                {
                    Run run = new Run()
                    {
                        File=fs.FullName
            
                    };
                    SocketManager.Send(BufferFormatV2.FormatFCA(run, Deflate.Compress));

                }
            }
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            GotoDir();
        }

        private void _Down_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                FileSystem downFile = (this.listView1.SelectedItems[this.listView1.SelectedItems.Count - 1].Tag as FileSystem);

                if (downFile != null)
                {
                    Down down = new Down()
                    {
                        FullName=downFile.FullName                       

                    };

                    SocketManager.Send(BufferFormatV2.FormatFCA(down, Deflate.Compress));

                }

            }
        }

        private void _Up_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;

               

                FileInfo file = new FileInfo(filename);

                if (file.Exists)
                {

                    FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                Re:
                    long key = DateTime.Now.Ticks;

                    if (UpFileList.ContainsKey(key))
                    {
                        System.Threading.Thread.Sleep(1);
                        goto Re;

                    }
                    UpFileList.Add(key, stream);



                    string upfilename = System.IO.Path.Combine(CurrentDir, file.Name);
                  

                    UpFile upfile = new UpFile()
                    {
                        FullName = upfilename,
                        Size = stream.Length,
                        UpKey = key,
                    };


                    SocketManager.Send(BufferFormatV2.FormatFCA(upfile, Deflate.Compress));
                }

            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }

            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                foreach (var item in ((System.Array)e.Data.GetData(DataFormats.FileDrop)))
                {
                    
                    string str = item.ToString();

                    UpFileEx(CurrentDir, str);
                }


            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                MessageBox.Show((e.Data.GetData(DataFormats.Text)).ToString(), "提示信息", MessageBoxButtons.OK); 
            }
        }


        private void UpFileEx(string cudir,string name)
        {


            FileInfo file = new FileInfo(name);

            if (file.Exists)
            {

                FileStream stream = new FileStream(name, FileMode.Open, FileAccess.Read);

            Re:
                long key = DateTime.Now.Ticks;

                if (UpFileList.ContainsKey(key))
                {
                    System.Threading.Thread.Sleep(1);
                    goto Re;

                }
                UpFileList.Add(key, stream);

                string upfilename = System.IO.Path.Combine(cudir,file.Name);

                UpFile upfile = new UpFile()
                {
                    FullName = upfilename,
                    Size = stream.Length,
                    UpKey = key,
                };


                SocketManager.Send(BufferFormatV2.FormatFCA(upfile, Deflate.Compress));
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(name);

                if (dir.Exists)
                {
                    string fullname =System.IO.Path.Combine(cudir,dir.Name);              


                    PackHandler.NewDir ndir = new PackHandler.NewDir()
                    {
                        DirName = fullname
                    };

                    SocketManager.Send(BufferFormatV2.FormatFCA(ndir, Deflate.Compress));


                    foreach (var item in dir.GetFileSystemInfos())
                    {
                        UpFileEx(fullname, item.FullName);
                    }
                }

            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;            
            this.ShowInTaskbar = true;
            this.Visible = true;
        }


     
        private void WinMain_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Visible = false;
              
            }

            GotoDir();

        }

   
      
    }
}
