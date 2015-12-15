using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using P2PCLIENT;
using ZYSocket.share;
using P2PFileInfo.Pack;
using System.IO;
namespace P2PFileInfo
{
    public partial class Form1 : Form
    {
        private ClientInfo MClient { get; set; }

        private List<UserInfo> UserList { get; set; }

        public UserInfo CurrentClient { get; set; }

        static int xlengt = 4096;
        static int ylengt = 16384;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            LogOut.Action+=new ActionOutHandler(LogOut_Action);
            MClient = new ClientInfo(Config.Default.ServerIP, Config.Default.ServerPort, Config.Default.ServerRegPort, Config.Default.MinPort, Config.Default.MaxPort, Config.Default.ResCount,Config.Default.Mac);

            MClient.ClientDataIn += new ClientDataInHandler(client_ClientDataIn);
            MClient.ClientConnToMe += new ClientConnToHandler(client_ClientConnToMe);
            MClient.ClientDiscon += new ClientDisconHandler(client_ClientDiscon);
            MClient.ConToServer();
        }



        private void UpdateUserListView()
        {
            this.BeginInvoke(new EventHandler((a, b) =>
               {
                   this.listView1.Items.Clear();

                   foreach (UserInfo user in UserList)
                   {
                       ListViewItem p = new ListViewItem();
                       p.Tag = user;
                       p.Text = user.ToString();
                       this.listView1.Items.Add(p);
                   }
               }));
        }



        void client_ClientDiscon(ConClient client, string message)
        {
            try
            {
                this.BeginInvoke(new EventHandler((a, b) =>
                    {
                        this.richTextBox1.AppendText(client.Host + ":" + client.Port + "-" + client.Key + " ->" + message + "\r\n");
                    }));

                if (client.UserToken != null)
                {
                    UserInfo user = client.UserToken as UserInfo;

                    if (user != null)
                    {
                        UserList.Remove(user);
                        UpdateUserListView();
                    }

                    client.UserToken = null;
                }

                if (!client.IsProxy)
                {
                    client.Sock.Close();
                }
            }
            catch
            {

            }
        }

        void client_ClientConnToMe(ConClient client)
        {
            if (UserList == null)
                UserList = new List<UserInfo>();

            UserInfo user = new UserInfo();
            user.Client = client;
            user.MainClient = MClient;
            user.Stream = new ZYSocket.share.ZYNetBufferReadStreamV2(1073741824);
            client.UserToken = user;
           
            UserList.Add(user);
            UpdateUserListView();

            this.BeginInvoke(new EventHandler((a, b) =>
            {
                this.richTextBox1.AppendText(client.Host + ":" + client.Port + "-" + client.Key + " 连接");
            }));
        }

        void client_ClientDataIn(string key,ConClient client, byte[] data)
        {
            try
            {
                UserInfo user = client.UserToken as UserInfo;

                if (user == null && client.IsProxy)
                {

                    if (UserList == null)
                        UserList = new List<UserInfo>();

                    user = new UserInfo();
                    user.Client = client;
                    user.MainClient = MClient;
                    user.Stream = new ZYSocket.share.ZYNetBufferReadStreamV2(1073741824);
                    client.UserToken = user;

                    UserList.Add(user);
                    UpdateUserListView();

                    this.BeginInvoke(new EventHandler((a, b) =>
                    {
                        this.richTextBox1.AppendText(client.Host + ":" + client.Port + "-" + client.Key + " 连接");
                    }));
                }



                if (user != null)
                {

                    user.Stream.Write(data);

                    byte[] datax;
                    while (user.Stream.Read(out datax))
                    {
                        if (user.IsSuccess)
                        {
                            DataOn(user, datax);
                        }
                        else
                        {
                            SuccessData(user, datax);
                        }
                    }

                }
                else
                {
                    if (!client.IsProxy)
                    {
                        client.Sock.Close();
                    }
                }
            }
            catch (Exception er)
            {
                this.BeginInvoke(new EventHandler((a, b) =>
                {
                    this.richTextBox1.AppendText(er.ToString() + "\r\n");
                }));
            }
        }

        private void SuccessData(UserInfo user, byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;
            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                FileCmd Fcmd = (FileCmd)cmd;
                switch (Fcmd)
                {
                    case FileCmd.Success:
                        {
                            Success success;

                            if (read.ReadObject<Success>(out success))
                            {
                                if (!success.IsRes)
                                {

                                    if (success.Key!=null&&success.Key.Equals(Config.Default.ConnentKey, StringComparison.Ordinal))
                                    {
                                        user.IsSuccess = true;
                                        success.IsSuccess = true;
                                        success.IsRes = true;
                                        MClient.SendData(user.Client.Key,BufferFormatV2.FormatFCA(success));

                                        UpdateUserListView();

                                        this.BeginInvoke(new EventHandler((a, b) =>
                                            {
                                                if(!user.Client.IsProxy)
                                                    this.richTextBox1.AppendText(user.Client.Host + ":" + user.Client.Port + " 登入成功\r\n");
                                                else
                                                    this.richTextBox1.AppendText(user.Client.Key + " 登入成功\r\n");
                                            }));
                                    }
                                    else
                                    {
                                        user.IsSuccess = false;
                                        success.IsSuccess = false;
                                        success.IsRes = true;
                                        MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(success));
                                    }
                                }
                                else
                                {
                                    if (!success.IsSuccess)
                                    {
                                        user.IsValidate = false;
                                        UpdateUserListView();
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            if (!user.Client.IsProxy)
                                                MessageBox.Show("连接到:" + user.Client.Host + ":" + user.Client.Port + " 密码错误");
                                            else
                                                MessageBox.Show("连接到:" + user.Client.Key + " 密码错误");

                                         
                                        }));
                                    }
                                    else
                                    {
                                        user.IsValidate = true;
                                        UpdateUserListView();
                                    }
                                }
                            }
                        }
                        break;
                    case FileCmd.GetFile:
                        {
                            GetFile dir;

                            if (read.ReadObject<GetFile>(out dir))
                            {
                                if (dir.IsRes)
                                { 
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        if (dir.IsSuccess)
                                        {
                                            this.listView2.Clear();

                                            dir.FileSystemList.Add(new FileSystem()
                                            {
                                                FileType = FileType.Dir,
                                                EditTime = DateTime.Now,
                                                FullName = dir.DirName,
                                                Name =  "..",
                                                Size = 0
                                            });

                                            dir.FileSystemList.Sort(new Comparison<FileSystem>((a1, a2) =>
                                            {
                                                if (a1.FileType == FileType.Dir && a2.FileType == FileType.Dir)
                                                {
                                                    return a1.Name.CompareTo(a2.Name);

                                                }
                                                else if (a1.FileType != FileType.Dir && a2.FileType != FileType.Dir)
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

                                            int x1 = 0, x2 = 0;
                                            foreach (FileSystem p in dir.FileSystemList)
                                            {
                                                if (p.FileType == FileType.File)
                                                {

                                                    this.listView2.Items.Add(new ListViewItem()
                                                    {
                                                        Text = p.Name,
                                                        ImageIndex = GetImageIndex(p.Name),
                                                        Tag = p

                                                    });
                                                    x1++;
                                                }
                                                else
                                                {
                                                    this.listView2.Items.Add(new ListViewItem()
                                                    {
                                                        Text = p.Name,
                                                        ImageIndex = 0,
                                                        Tag = p
                                                    });

                                                    x2++;
                                                }

                                            }
                                        }
                                    }));

                                }

                            }

                        }
                        break;
                    case FileCmd.Down:
                        {
                            Down down;
                            if (read.ReadObject<Down>(out down))
                            {
                                if (down.IsRes)
                                {                              
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        if (down.IsSuccess)
                                        {
                                            DownFile downwin = new DownFile(user, down);
                                            downwin.Show();

                                        }
                                        else
                                        {
                                            MessageBox.Show(down.Msg);
                                        }

                                    }));
                                }
                            }
                        }
                        break;
                    default:
                        {
                            if (user.DownDataOn != null)
                            {
                                user.DownDataOn(data);
                            }
                        }
                        break;
                }
            }
        }

        private void DataOn(UserInfo user, byte[] data)
        {

            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;
            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                FileCmd Fcmd = (FileCmd)cmd;

                switch (Fcmd)
                {
                    case FileCmd.Success:
                        {
                            Success success;

                            if (read.ReadObject<Success>(out success))
                            {
                                if (!success.IsRes)
                                {

                                    if (success.Key!=null&&success.Key.Equals(Config.Default.ConnentKey, StringComparison.Ordinal))
                                    {
                                        user.IsSuccess = true;
                                        success.IsSuccess = true;
                                        success.IsRes = true;                                       
                                        MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(success));
                                        UpdateUserListView();
                                    }
                                    else
                                    {
                                        user.IsSuccess = false;
                                        success.IsSuccess = false;
                                        success.IsRes = true;
                                        MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(success));
                                    }
                                }
                                else
                                {
                                    if (!success.IsSuccess)
                                    {
                                        user.IsValidate = false;
                                        UpdateUserListView();
                                        this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            if (!user.Client.IsProxy)
                                                MessageBox.Show("连接到:" + user.Client.Host + ":" + user.Client.Port + " 密码错误");
                                            else
                                                MessageBox.Show("连接到:" + user.Client.Key + " 密码错误");
                                           
                                        }));
                                    }
                                    else
                                    {
                                        user.IsValidate = true;
                                        UpdateUserListView();
                                    }
                                }
                            }
                        }
                        break;
                    case FileCmd.GetFile:
                        {
                            GetFile dir;

                            if (read.ReadObject<GetFile>(out dir))
                            {
                                if (!dir.IsRes)
                                {


                                    string dirname = dir.DirName;

                                    if (string.IsNullOrEmpty(dirname))
                                    {
                                        dirname = Config.Default.SharePath;
                                       
                                    }


                                    DirectoryInfo dirinfo = new DirectoryInfo(dirname);

                                    if (dirinfo.Parent != null)
                                    {
                                        dir.DirName = dirinfo.Parent.FullName;
                                    }
                                    else
                                    {
                                        dir.DirName = Config.Default.SharePath;
                                    }
                                    if (!(dirinfo.FullName.IndexOf(Config.Default.SharePath) == 0))
                                    {
                                        dir.IsSuccess = false;
                                        dir.Msg = "无法找到目录:" + dirinfo.FullName;                                      
                                        MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(dir));
                                        return;
                                    }

                                    dir.IsRes = true;
                                    if (dirinfo.Exists)
                                    {
                                        dir.FileSystemList = new List<FileSystem>();

                                        FileSystemInfo[] files = dirinfo.GetFileSystemInfos();

                                        foreach (var p in files)
                                        {
                                            FileSystem tmp = new FileSystem()
                                            {
                                                Name = p.Name,
                                                FullName = p.FullName,
                                                FileType = p is DirectoryInfo ? FileType.Dir : FileType.File,
                                                Size = p is DirectoryInfo ? 0 : (p as FileInfo).Length,
                                                EditTime = p.LastWriteTime
                                            };

                                            dir.FileSystemList.Add(tmp);

                                        }

                                        dir.IsSuccess = true;
                                    }
                                    else
                                    {
                                        dir.IsSuccess = false;
                                        dir.Msg = "无法找到目录:" + dirname;

                                    }
                                                                      
                                    MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(dir));
                                }
                                else
                                {
                                     this.BeginInvoke(new EventHandler((a, b) =>
                                       {
                                           if (dir.IsSuccess)
                                           {
                                               this.listView2.Clear();

                                               dir.FileSystemList.Add(new FileSystem()
                                               {
                                                   FileType=FileType.Dir,
                                                   EditTime=DateTime.Now,
                                                   FullName = dir.DirName,
                                                   Name="..",
                                                   Size=0
                                               });

                                               dir.FileSystemList.Sort(new Comparison<FileSystem>((a1, a2) =>
                                               {
                                                   if (a1.FileType == FileType.Dir && a2.FileType == FileType.Dir)
                                                   {
                                                       return a1.Name.CompareTo(a2.Name);

                                                   }
                                                   else if (a1.FileType != FileType.Dir && a2.FileType != FileType.Dir)
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

                                               int x1 = 0, x2 = 0;
                                               foreach (FileSystem p in dir.FileSystemList)
                                               {
                                                   if (p.FileType == FileType.File)
                                                   {

                                                       this.listView2.Items.Add(new ListViewItem()
                                                       {
                                                           Text = p.Name,
                                                           ImageIndex = GetImageIndex(p.Name),
                                                           Tag = p

                                                       });
                                                       x1++;
                                                   }
                                                   else
                                                   {
                                                       this.listView2.Items.Add(new ListViewItem()
                                                       {
                                                           Text = p.Name,
                                                           ImageIndex = 0,
                                                           Tag = p
                                                       });

                                                       x2++;
                                                   }

                                               }



                                           }
                                       }));

                                }
                            }
                        }
                        break;
                    case FileCmd.Down:
                        {
                            Down down;
                            if (read.ReadObject<Down>(out down))
                            {
                                if (!down.IsRes)
                                {
                                    down.IsRes = true;

                                    FileInfo file = new FileInfo(down.FullName);

                                    if (file.Exists)
                                    {
                                        down.Size = file.Length;

                                    Re:
                                        long key = DateTime.Now.Ticks;

                                        if (user.DownKeyList.ContainsKey(key))
                                        {
                                            System.Threading.Thread.Sleep(1);
                                            goto Re;
                                        }
                                        user.DownKeyList.Add(key, file.FullName);

                                        down.DownKey = key;
                                        down.IsSuccess = true;



                                    }
                                    else
                                    {
                                        down.IsSuccess = false;
                                        down.Msg = "未找到文件:" + down.FullName;
                                    }                                

                                    MClient.SendData(user.Client.Key, BufferFormatV2.FormatFCA(down));

                                    this.BeginInvoke(new EventHandler((a, b) =>
                                        {
                                            if(!user.Client.IsProxy)
                                                this.richTextBox1.AppendText(user.Client.Host + ":" + user.Client.Port + " 下载文件:" + file.FullName + "\r\n");
                                            else
                                                this.richTextBox1.AppendText(user.Client.Key + " 下载文件:" + file.FullName + "\r\n");
                                        }));

                                }
                                else
                                {
                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        if (down.IsSuccess)
                                        {
                                            DownFile downwin = new DownFile(user,down);                                            
                                            downwin.Show();

                                        }
                                        else
                                        {
                                            MessageBox.Show(down.Msg);
                                        }

                                    }));
                                }
                            }
                        }
                        break;
                    case FileCmd.DownNow:
                        {
                            long downkey;

                            if (read.ReadInt64(out downkey))
                            {
                                if (user.DownKeyList.ContainsKey(downkey))
                                {
                                    string filename = user.DownKeyList[downkey];
                                    user.DownKeyList.Remove(downkey);

                                    if (File.Exists(filename))
                                    {
                                        FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                                        lock (user.Stream)
                                        {
                                            user.StreamList.Add(downkey, stream);
                                        }

                                        System.Threading.ThreadPool.QueueUserWorkItem((a) =>
                                            {


                                                try
                                                {
                                                    byte[] buffa = new byte[xlengt];

                                                    int r = 0;
                                                    long p = 0;
                                                    do
                                                    {
                                                        try
                                                        {
                                                            r = stream.Read(buffa, 0, buffa.Length);

                                                            if (r < buffa.Length && r > 0)
                                                            {
                                                                byte[] buffb = new byte[r];

                                                                Buffer.BlockCopy(buffa, 0, buffb, 0, buffb.Length);

                                                                BufferFormatV2 buff = new BufferFormatV2(2002);
                                                                buff.AddItem(downkey);
                                                                buff.AddItem(p);
                                                                buff.AddItem(p + r - 1);
                                                                buff.AddItem(buffb);
                                                                

                                                                MClient.SendData(user.Client.Key, buff.Finish());

                                                                break;
                                                            }
                                                            else if (r > 0)
                                                            {
                                                                BufferFormatV2 buff = new BufferFormatV2(2002);
                                                                buff.AddItem(downkey);
                                                                buff.AddItem(p);
                                                                buff.AddItem(p + r - 1);
                                                                buff.AddItem(buffa);
                                                                MClient.SendData(user.Client.Key, buff.Finish());

                                                                p += r;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            break;
                                                        }

                                                    } while (r > 0);


                                                    BufferFormatV2 buffcheck = new BufferFormatV2(2003);
                                                    buffcheck.AddItem(downkey);                                                
                                                    MClient.SendData(user.Client.Key, buffcheck.Finish());

                                                }
                                                catch (Exception er)
                                                {
                                                    stream.Close();                                                  
                                                    lock (user.StreamList)
                                                    {
                                                        user.StreamList.Remove(downkey);
                                                    }

                                                    BufferFormatV2 buff = new BufferFormatV2(2001);
                                                    buff.AddItem(downkey);
                                                    buff.AddItem(er.Message);                                                   
                                                    MClient.SendData(user.Client.Key, buff.Finish());
                                                }

                                            }, null);
                                  
                                    }
                                    else
                                    {
                                        BufferFormatV2 buff = new BufferFormatV2(2001);
                                        buff.AddItem(downkey);
                                        buff.AddItem("文件不存在");                                       
                                        MClient.SendData(user.Client.Key, buff.Finish());
                                    }

                                }
                                else
                                {
                                    BufferFormatV2 buff = new BufferFormatV2(2001);
                                    buff.AddItem(downkey);
                                    buff.AddItem("DownKey 不存在");
                                    MClient.SendData(user.Client.Key, buff.Finish());
                                }

                            }

                        }
                        break;
                    case FileCmd.DownClose:
                        {
                            long downkey;

                            if (read.ReadInt64(out downkey))
                            {
                                if (user.DownKeyList.ContainsKey(downkey))
                                    user.DownKeyList.Remove(downkey);

                                if (user.StreamList.ContainsKey(downkey))
                                {
                                    FileStream strem;

                                    lock (user.StreamList)
                                    {
                                        strem = user.StreamList[downkey];
                                        user.StreamList.Remove(downkey);
                                    }

                                    strem.Close();


                                    this.BeginInvoke(new EventHandler((a, b) =>
                                    {
                                        

                                        if (!user.Client.IsProxy)
                                            this.richTextBox1.AppendText(user.Client.Host + ":" + user.Client.Port + " 下载文件完毕!" + "\r\n");
                                        else
                                            this.richTextBox1.AppendText(user.Client.Key + " 下载文件完毕!" + "\r\n");
                                    }));
                                }


                            }

                        }
                        break;
                    case FileCmd.ReBytes:
                        {
                            long downkey;

                            if (read.ReadInt64(out downkey))
                            {
                                long startpostion;
                                int size;

                                if (read.ReadInt64(out startpostion) && read.ReadInt32(out size))
                                {
                                    if (user.StreamList.ContainsKey(downkey))
                                    {
                                        FileStream strem = user.StreamList[downkey];

                                        strem.Position = startpostion;

                                        byte[] xdata = new byte[size];

                                        strem.Read(xdata, 0, xdata.Length);


                                        BufferFormatV2 buff = new BufferFormatV2(2004);
                                        buff.AddItem(downkey);
                                        buff.AddItem(startpostion);
                                        buff.AddItem(xdata);                                       
                                        MClient.SendData(user.Client.Key, buff.Finish());

                                    }
                                    else
                                    {
                                        BufferFormatV2 buff = new BufferFormatV2(2001);
                                        buff.AddItem(downkey);
                                        buff.AddItem("DownKey 不存在");
                                        MClient.SendData(user.Client.Key, buff.Finish());
                                    }

                                }
                            }

                        }
                        break;
                    default:
                        {
                            if (user.DownDataOn != null)
                            {
                                user.DownDataOn(data);
                            }
                        }
                        break;

                }
            }


        }

        private int GetImageIndex(string name)
        {
            if (name.IndexOf(".exe") > 0)
            {
                return 2;
            }
            else if (name.IndexOf(".dll") > 0)
            {
                return 3;
            }
            else
            {
                return 1;
            }

        }

        void LogOut_Action(string message, ActionType type)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
                  {
                      this.richTextBox1.AppendText(message + "\r\n");
                  }));
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];

                if (item.Tag != null)
                {
                    UserInfo tmp = item.Tag as UserInfo;

                    CurrentClient = tmp;
                }
            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];

                if (item.Tag != null)
                {
                    UserInfo tmp = item.Tag as UserInfo;

                    if (tmp != null)
                    {

                        CurrentClient = tmp;

                        if (!tmp.IsValidate)
                        {
                            LogOn tmpwin = new LogOn();
                            tmpwin.ShowDialog();

                            string password = tmpwin.PassWrod;

                            Success logon = new Success()
                            {
                                Key = password
                            };

                            tmp.SendData(BufferFormatV2.FormatFCA(logon));

                        }
                        else
                        {
                            GetFile getfile = new GetFile();

                            tmp.SendData(BufferFormatV2.FormatFCA(getfile));
                        }
                    }
                }
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                ListViewItem item = listView2.SelectedItems[listView2.SelectedItems.Count - 1];

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

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0 && CurrentClient!=null)
            {
                ListViewItem item = listView2.SelectedItems[listView2.SelectedItems.Count - 1];
                string DirName = "";

                FileSystem dirinfo = item.Tag as FileSystem;
                if (dirinfo.FileType == FileType.Dir)
                {
                    DirName = dirinfo.FullName;
                }
                else
                {
                    if (MessageBox.Show("是否下载文件:" + dirinfo.Name + "?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                         FileSystem downFile = (this.listView2.SelectedItems[this.listView2.SelectedItems.Count - 1].Tag as FileSystem);

                         if (downFile != null)
                         {
                             Down down = new Down()
                              {
                                  FullName = downFile.FullName

                              };

                             CurrentClient.SendData(BufferFormatV2.FormatFCA(down));

                         }
                    }
                    return;
                }

                GetFile tmp = new GetFile()
                {
                    DirName = DirName,
                    FileSystemList = new List<FileSystem>(),
                    IsSuccess = false,
                    Msg = ""
                };

                CurrentClient.SendData(BufferFormatV2.FormatFCA(tmp));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MClient.ResetConnClient();
           
        }

      
    }
}
