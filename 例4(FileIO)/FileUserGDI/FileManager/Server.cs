using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZYSocket.Server;
using ZYSocket.share;
using System.Net.Sockets;
using PackHandler;
using ZYSocket.Compression;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Xml.Serialization;
using System.Xml;

namespace FileManager
{
    public partial class Server : Form
    {
        //建立一个ZYSOCKETSERVER 对象 注意启动前应该先设置 App.Config 文件,
        //如果你不想设置App.Config文件 那么可以在构造方法里面传入相关的设置
        ZYSocketSuper server = new ZYSocketSuper();
    
        int xlengt = 4096;
        int ylengt = 16384;

        public List<User> UserList { get; set; }

        public Server()
        {
            InitializeComponent();
        }




        private void Server_Load(object sender, EventArgs e)
        {
            LoadingUser();          

            BufferFormatV2.ObjFormatType = BuffFormatType.XML;
            ReadBytesV2.ObjFormatType = BuffFormatType.XML;
            server.BinaryOffsetInput = new BinaryInputOffsetHandler(BinaryInputHandler); //设置输入代理
            server.Connetions = new ConnectionFilter(ConnectionFilter); //设置连接代理
            server.MessageInput = new MessageInputHandler(MessageInputHandler); //设置 客户端断开
            server.IsOffsetInput = true;
            server.Start(); //启动服务器

            this.WindowState = FormWindowState.Minimized;
        }


        public void SaveUser()
        {
            if (File.Exists("user.xml"))
            {
                File.Delete("user.xml");
            }

            StringBuilder output = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(UserList.GetType());
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.Unicode
            };
            XmlWriter xmlWriter = XmlWriter.Create(output, settings);
            serializer.Serialize(xmlWriter, UserList);
            xmlWriter.Close();
            byte[] data= Encoding.UTF8.GetBytes(output.ToString());

            File.WriteAllBytes("user.xml", data);

        }


        public void LoadingUser()
        {
            if (File.Exists("user.xml"))
            {
                byte[] data = File.ReadAllBytes("user.xml");

                string s = Encoding.UTF8.GetString(data);
                List<User> obj3 = new List<User>();
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (Stream stream2 = new MemoryStream(Encoding.Unicode.GetBytes(s)))
                {
                    XmlReader xmlReader = XmlReader.Create(stream2);
                    obj3 = (List<User>)serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                this.listView1.Items.Clear();

                UserList = obj3;

                foreach (var item in UserList)
                {

                    ListViewItem tmp = new ListViewItem(item.UserName);
                    tmp.SubItems.Add(item.Password);
                    tmp.SubItems.Add(item.Path);
                    this.listView1.Items.Add(tmp);
                }
            }
        }





        /// <summary>
        /// 用户断开代理（你可以根据socketAsync 读取到断开的
        /// </summary>
        /// <param name="message">断开消息</param>
        /// <param name="socketAsync">断开的SOCKET</param>
        /// <param name="erorr">错误的ID</param>
        void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            try
            {
                
                AddLog(message);

                if (socketAsync.UserToken != null)
                {
                    UserManager users = socketAsync.UserToken as UserManager;

                    if (users != null)
                    {
                        foreach (KeyValuePair<long, FileStream> x in users.StreamList)
                        {
                            x.Value.Close();
                        }

                        users.StreamList.Clear();
                        users.DownKeyList.Clear();


                        foreach (KeyValuePair<long, FileStream> x in users.UpFileList)
                        {
                            x.Value.Close();
                        }

                        users.UpFileList.Clear();
                        users.IsCheckTable.Clear();

                        AddLog(users.UserName + " 退出登入");

                    }

                    socketAsync.UserToken = null;
                }
                socketAsync.AcceptSocket.Close();
            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }
        }

        private void AddLog(string msg)
        {
            this.richTextBox1.BeginInvoke(new EventHandler((a, b) =>
                {
                    this.richTextBox1.AppendText(msg+"\r\n");
                }));

        }



        /// <summary>
        /// 用户连接的代理
        /// </summary>
        /// <param name="socketAsync">连接的SOCKET</param>
        /// <returns>如果返回FALSE 则断开连接,这里注意下 可以用来封IP</returns>
        bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            try
            {
                AddLog(string.Format("UserConn {0}", socketAsync.AcceptSocket.RemoteEndPoint.ToString()));
                socketAsync.UserToken = new ZYNetRingBufferPoolV2(40960);
                return true;
            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
                return false;
            }
        }

        /// <summary>
        /// 数据包输入
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="socketAsync">该数据包的通讯SOCKET</param>
        void BinaryInputHandler(byte[] data,int offset,int count, SocketAsyncEventArgs socketAsync)
        {
            try
            {
                ZYNetRingBufferPoolV2 stream = socketAsync.UserToken as ZYNetRingBufferPoolV2;

                if (stream != null)
                {
                    //最新的数据包整合类

                    stream.Write(data, offset,count);

                    byte[] datax;
                    while (stream.Read(out datax))
                    {
                        LogOnPack(datax, socketAsync);
                    }
                }
                else
                {
                    UserManager userinfo = socketAsync.UserToken as UserManager;

                    userinfo.Stream.Write(data, offset, count);

                    byte[] datax;
                    while (userinfo.Stream.Read(out datax))
                    {
                        DataOn(datax, userinfo);
                    }



                }

            }
            catch (Exception er)
            {
                AddLog(er.ToString());
                server.Disconnect(socketAsync.AcceptSocket);
            }


        }

        /// <summary>
        /// 验证登入
        /// </summary>
        /// <param name="data"></param>
        /// <param name="e"></param>
        void LogOnPack(byte[] data, SocketAsyncEventArgs e)
        {
            ReadBytesV2 read = new ReadBytesV2(data, Deflate.Decompress);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && lengt == read.Length && read.ReadInt32(out cmd))
            {
                PackType cmdtype = (PackType)cmd;

                switch (cmdtype)
                {
                    case PackType.LogOn:
                        {
                            LogOn logOn;
                            if (read.ReadObject<LogOn>(out logOn))
                            {
                                User user;

                                if ((user=UserList.Find(p=>p.UserName==logOn.UserName&&p.Password==p.Password))!=null)
                                {


                                    LogOnRes res = new LogOnRes()
                                    {
                                        Path = user.Path,
                                        IsOk = true,
                                        Msg = "登入成功"
                                    };

                                    UserManager userinfo = new UserManager()
                                    {
                                        Userinfo=user,
                                        UserName=user.UserName,
                                        Asyn = e,
                                        Stream = e.UserToken as ZYNetRingBufferPoolV2
                                    };

                                    e.UserToken = userinfo;


                                    server.SendData(e.AcceptSocket, BufferFormatV2.FormatFCA(res, Deflate.Compress));

                                    AddLog(logOn.UserName + " 登入成功");

                                }
                                else
                                {
                                    LogOnRes res = new LogOnRes()
                                    {
                                        IsOk = false,
                                        Msg = "用户名或密码错误"
                                    };



                                    server.SendData(e.AcceptSocket, BufferFormatV2.FormatFCA(res, Deflate.Compress));
                                }

                            }
                        }
                        break;

                }
            }
        }

        void DataOn(byte[] data, UserManager user)
        {

            ReadBytesV2 read = new ReadBytesV2(data, Deflate.Decompress);

            int lengt;
            int cmd;

            if (!read.ReadInt32(out lengt) || !read.ReadInt32(out cmd) || cmd == 0)
                read = new ReadBytesV2(data);
            else
                read.Postion = 0;

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
                                string dirname = dir.DirName;

                                if (string.IsNullOrEmpty(dirname) || dirname == "/")
                                {
                                    dirname = user.Userinfo.Path;
                                    dir.DirName = dirname;
                                }



                                DirectoryInfo dirinfo = new DirectoryInfo(dirname);

                                if (dirinfo.Exists)
                                {
                                    string name = dirinfo.FullName;

                                    if (name.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        dir.IsSuccess = false;
                                        dir.Msg = "无法找到目录:" + dir.DirName;
                                        server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(dir, Deflate.Compress));
                                        return;
                                    }


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
                                    dir.Msg = "无法找到目录:" + dir.DirName;

                                }

                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(dir, Deflate.Compress));

                            }

                        }
                        break;
                    case PackType.DelFile:
                        {
                            DelFile dfile;
                            if (read.ReadObject<DelFile>(out dfile))
                            {

                                foreach (DelFileName x in dfile.DelFileList)
                                {

                                    if (x.FullName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        x.IsSuccess = false;
                                        x.Msg = "没有找到:" + x.FullName;
                                        continue;
                                    }


                                    if (x.FType == FileType.Dir)
                                    {
                                        if (Directory.Exists(x.FullName))
                                        {

                                            Directory.Delete(x.FullName, true);

                                            x.IsSuccess = true;
                                        }
                                        else
                                        {
                                            x.IsSuccess = false;
                                            x.Msg = "没有找到目录:" + x.FullName;
                                        }

                                    }
                                    else
                                    {
                                        if (File.Exists(x.FullName))
                                        {
                                            try
                                            {
                                                File.Delete(x.FullName);
                                                x.IsSuccess = true;

                                                AddLog(user.UserName + " 删除文件"+x.FullName);
                                            }
                                            catch (Exception er)
                                            {
                                                x.IsSuccess = false;
                                                x.Msg = "删除文件 " + x.FullName + "发生错误:" + er.Message;
                                            }
                                        }
                                        else
                                        {
                                            x.IsSuccess = false;
                                            x.Msg = "没有找到文件:" + x.FullName;
                                        }

                                    }

                                }

                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(dfile, Deflate.Compress));

                            }
                        }
                        break;
                    case PackType.NewDir:
                        {
                            NewDir ndir;
                            if (read.ReadObject<NewDir>(out ndir))
                            {
                                try
                                {
                                    DirectoryInfo _ndir = new DirectoryInfo(ndir.DirName);

                                    if (_ndir.FullName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        ndir.IsSuccess = false;
                                        ndir.Msg = "无法创建" + ndir.DirName;
                                        server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(ndir, Deflate.Compress));
                                        return;
                                    }

                                    Directory.CreateDirectory(_ndir.FullName);
                                    ndir.IsSuccess = true;
                                    AddLog(user.UserName + " 创建文件夹" + _ndir.FullName);
                                }
                                catch (Exception er)
                                {
                                    ndir.IsSuccess = false;
                                    ndir.Msg = er.Message;
                                }
                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(ndir, Deflate.Compress));

                            }

                        }
                        break;
                    case PackType.MoveFileSystem:
                        {
                            MoveFileSystem mfs;

                            if (read.ReadObject<MoveFileSystem>(out mfs))
                            {
                                if ((mfs.OldName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0) || (mfs.NewName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0))
                                {
                                    mfs.Msg = "无法移动，没有权限";
                                    mfs.IsSuccess = false;
                                    server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(mfs, Deflate.Compress));
                                    return;
                                }

                                if (mfs.FileType == FileType.Dir)
                                {
                                    try
                                    {
                                        DirectoryInfo dirinfo = new DirectoryInfo(mfs.OldName);
                                        dirinfo.MoveTo(mfs.NewName);
                                        mfs.IsSuccess = true;

                                        AddLog(user.UserName + " 移动文件 " + mfs.OldName + "->" + mfs.NewName);
                                    }
                                    catch (Exception er)
                                    {
                                        mfs.Msg = er.Message;
                                        mfs.IsSuccess = false;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        FileInfo fileinfo = new FileInfo(mfs.OldName);
                                        fileinfo.MoveTo(mfs.NewName);
                                        mfs.IsSuccess = true;
                                    }
                                    catch (Exception er)
                                    {
                                        mfs.Msg = er.Message;
                                        mfs.IsSuccess = false;
                                    }


                                }

                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(mfs, Deflate.Compress));
                            }
                        }
                        break;
                    case PackType.Run:
                        {
                            Run run;
                            if (read.ReadObject<Run>(out run))
                            {
                                try
                                {

                                    // System.Diagnostics.Process.Start(run.File, run.Arge);
                                    run.Msg = "不支持运行";
                                    run.IsSuccess = false;
                                }
                                catch (Exception er)
                                {
                                    run.Msg = er.Message;
                                    run.IsSuccess = false;
                                }

                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(run, Deflate.Compress));
                            }
                        }
                        break;
                    case PackType.Down:
                        {
                            Down down;
                            if (read.ReadObject<Down>(out down))
                            {
                                FileInfo file = new FileInfo(down.FullName);

                                if (file.Exists)
                                {
                                    if (file.FullName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        down.IsSuccess = false;
                                        down.Msg = "没有权限";
                                        server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(down, Deflate.Compress));
                                        return;
                                    }


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


                                    AddLog(user.UserName + " 下载文件 " + down.FullName);
                                }
                                else
                                {
                                    down.IsSuccess = false;
                                    down.Msg = "未找到文件:" + down.FullName;
                                }

                                server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(down, Deflate.Compress));
                            }
                        }
                        break;
                    case PackType.DownNow:
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
                                        if (filename.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            BufferFormatV2 buff = new BufferFormatV2(2001);
                                            buff.AddItem(downkey);
                                            buff.AddItem("没有权限");
                                            server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                                            return;
                                        }

                                        FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                                        user.StreamList.TryAdd(downkey, stream);


                                        Task SendFlie = new Task(new Action(() =>
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

                                                            user.Asyn.AcceptSocket.Send(buff.Finish());

                                                            break;
                                                        }
                                                        else if (r > 0)
                                                        {
                                                            BufferFormatV2 buff = new BufferFormatV2(2002);
                                                            buff.AddItem(downkey);
                                                            buff.AddItem(p);
                                                            buff.AddItem(p + r - 1);
                                                            buff.AddItem(buffa);
                                                            user.Asyn.AcceptSocket.Send(buff.Finish());

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
                                                user.Asyn.AcceptSocket.Send(buffcheck.Finish());

                                            }
                                            catch (Exception er)
                                            {
                                                stream.Close();
                                                FileStream strem;
                                                user.StreamList.TryRemove(downkey, out strem);

                                                BufferFormatV2 buff = new BufferFormatV2(2001);
                                                buff.AddItem(downkey);
                                                buff.AddItem(er.Message);
                                                server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                                            }

                                        }));

                                        SendFlie.Start();

                                    }
                                    else
                                    {
                                        BufferFormatV2 buff = new BufferFormatV2(2001);
                                        buff.AddItem(downkey);
                                        buff.AddItem("文件不存在");
                                        server.SendData(user.Asyn.AcceptSocket, buff.Finish());

                                    }

                                }
                                else
                                {
                                    BufferFormatV2 buff = new BufferFormatV2(2001);
                                    buff.AddItem(downkey);
                                    buff.AddItem("DownKey 不存在");
                                    server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                                }

                            }

                        }
                        break;
                    case PackType.DownClose:
                        {
                            long downkey;

                            if (read.ReadInt64(out downkey))
                            {
                                if (user.DownKeyList.ContainsKey(downkey))
                                    user.DownKeyList.Remove(downkey);

                                if (user.StreamList.ContainsKey(downkey))
                                {
                                    FileStream strem;

                                    user.StreamList.TryRemove(downkey, out strem);



                                    strem.Close();



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
                                        server.SendData(user.Asyn.AcceptSocket, buff.Finish());

                                    }
                                    else
                                    {
                                        BufferFormatV2 buff = new BufferFormatV2(2001);
                                        buff.AddItem(downkey);
                                        buff.AddItem("DownKey 不存在");
                                        server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                                    }

                                }
                            }

                        }
                        break;
                    case PackType.UpFile:
                        {
                            UpFile upFile;
                            if (read.ReadObject<UpFile>(out upFile))
                            {
                                if (!user.UpFileList.ContainsKey(upFile.UpKey))
                                {
                                    try
                                    {

                                        FileInfo _upfile = new FileInfo(upFile.FullName);

                                        if (_upfile.FullName.IndexOf(user.Userinfo.Path, StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            upFile.IsSuccess = false;
                                            upFile.Msg = "没有权限";
                                            server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(upFile, Deflate.Compress));
                                            return;
                                        }

                                        FileStream stream = new FileStream(_upfile.FullName, FileMode.Create, FileAccess.Write);
                                        stream.SetLength(upFile.Size);
                                        user.UpFileList.TryAdd(upFile.UpKey, stream);

                                        user.IsCheckTable.TryAdd(upFile.UpKey, LoadingCheckTable(upFile.Size));

                                        upFile.IsSuccess = true;

                                        server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(upFile, Deflate.Compress));

                                        AddLog(user.UserName + " 上传文件 " + upFile.FullName);

                                    }
                                    catch (Exception er)
                                    {
                                        upFile.IsSuccess = false;
                                        upFile.Msg = er.Message;
                                        server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(upFile, Deflate.Compress));
                                    }

                                }
                                else
                                {
                                    upFile.IsSuccess = false;
                                    upFile.Msg = "Key重复";

                                    server.SendData(user.Asyn.AcceptSocket, BufferFormatV2.FormatFCA(upFile, Deflate.Compress));
                                }
                            }
                        }
                        break;
                    case PackType.UpClose:
                        {
                            long key;
                            if (read.ReadInt64(out key))
                            {
                                if (user.UpFileList.ContainsKey(key))
                                {
                                    FileStream stream;
                                    user.UpFileList.TryRemove(key, out stream);
                                    stream.Close();
                                }

                                if (user.IsCheckTable.ContainsKey(key))
                                {
                                    List<CheckB> checkblist;
                                    user.IsCheckTable.TryRemove(key, out checkblist);

                                }


                                BufferFormatV2 buff = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                                buff.AddItem(key);
                                server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                            }
                        }
                        break;
                    case PackType.DateUp:
                        {
                            long Key;

                            if (read.ReadInt64(out Key) && user.UpFileList.ContainsKey(Key))
                            {
                                long startp;
                                long endp;
                                byte[] buff;

                                if (read.ReadInt64(out startp) && read.ReadInt64(out endp) && read.ReadByteArray(out buff))
                                {
                                    //Task task = new Task(() =>
                                    //    {
                                    try
                                    {
                                        if (user.IsCheckTable.ContainsKey(Key) && user.UpFileList.ContainsKey(Key))
                                        {
                                            List<CheckB> IsCheckTable = user.IsCheckTable[Key];
                                            FileStream FStream = user.UpFileList[Key];

                                            CheckB cb = IsCheckTable.Find(p => p.StartPostion == startp);

                                            if (cb != null)
                                            {
                                                if (cb.EndPostion == endp && buff.Length >= cb.Size)
                                                {
                                                    cb.Checkd = true;
                                                    FStream.Position = cb.StartPostion;
                                                    FStream.Write(buff, 0, cb.Size);
                                                }
                                            }
                                            else
                                            {

                                                if (user.UpFileList.ContainsKey(Key))
                                                {
                                                    FileStream stream;
                                                    user.UpFileList.TryRemove(Key, out stream);
                                                    stream.Close();
                                                }

                                                if (user.IsCheckTable.ContainsKey(Key))
                                                {
                                                    List<CheckB> checkblist;
                                                    user.IsCheckTable.TryRemove(Key, out checkblist);

                                                }

                                                Console.WriteLine("数据验证出错");
                                                BufferFormatV2 buffx = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                                                buffx.AddItem(Key);
                                                server.SendData(user.Asyn.AcceptSocket, buffx.Finish());

                                            }

                                        }
                                    }
                                    catch (Exception er)
                                    {
                                        Console.WriteLine(er.Message);
                                    }
                                    //    });

                                    //task.Start();

                                }
                            }
                            else
                            {
                                BufferFormatV2 buff = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                                buff.AddItem(Key);
                                server.SendData(user.Asyn.AcceptSocket, buff.Finish());
                            }
                        }
                        break;
                    case PackType.UpCheck:
                        {
                            long Key;

                            if (read.ReadInt64(out Key) && user.UpFileList.ContainsKey(Key))
                            {
                                //Task task = new Task(() =>
                                //           {
                                CheckDown(user, Key, user.IsCheckTable[Key]);
                                //           });

                                //task.Start();
                            }
                            else
                            {
                                BufferFormatV2 buffx = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                                buffx.AddItem(Key);
                                server.SendData(user.Asyn.AcceptSocket, buffx.Finish());
                            }
                        }
                        break;
                    case PackType.DataSet:
                        {
                            long Key;

                            if (read.ReadInt64(out Key) && user.UpFileList.ContainsKey(Key))
                            {
                                long startp;
                                byte[] buff;

                                if (read.ReadInt64(out startp) && read.ReadByteArray(out buff))
                                {
                                    //Task task = new Task(() =>
                                    //    {
                                    if (user.UpFileList.ContainsKey(Key) && user.IsCheckTable.ContainsKey(Key))
                                    {
                                        List<CheckB> IsCheckTable = user.IsCheckTable[Key];
                                        FileStream FStream = user.UpFileList[Key];

                                        CheckB cb = IsCheckTable.Find(p => p.StartPostion == startp);

                                        if (buff.Length >= cb.Size)
                                        {
                                            cb.Checkd = true;

                                            FStream.Position = cb.StartPostion;
                                            FStream.Write(buff, 0, cb.Size);

                                        }

                                        CheckDown(user, Key, IsCheckTable);

                                    }
                                    else
                                    {
                                        BufferFormatV2 buffx = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                                        buffx.AddItem(Key);
                                        server.SendData(user.Asyn.AcceptSocket, buffx.Finish());
                                    }


                                    //    });

                                    //task.Start();

                                }

                            }

                        }
                        break;

                }

            }


        }

        void CheckDown(UserManager user, long key, List<CheckB> IsCheckTable)
        {
            CheckB p = IsCheckTable.Find(x => x.Checkd == false);

            if (p == null)
            {

                if (user.UpFileList.ContainsKey(key))
                {
                    FileStream stream;
                    user.UpFileList.TryRemove(key, out stream);
                    stream.Close();
                }

                if (user.IsCheckTable.ContainsKey(key))
                {
                    List<CheckB> checkblist;
                    user.IsCheckTable.TryRemove(key, out checkblist);

                }

                BufferFormatV2 buffx = new BufferFormatV2((int)PackType.UpClose, Deflate.Compress);
                buffx.AddItem(key);
                server.SendData(user.Asyn.AcceptSocket, buffx.Finish());


            }
            else
            {
                BufferFormatV2 buff = new BufferFormatV2((int)PackType.ReBytes);
                buff.AddItem(key);
                buff.AddItem(p.StartPostion);
                buff.AddItem(p.Size);
                server.SendData(user.Asyn.AcceptSocket, buff.Finish());
            }
        }

        List<CheckB> LoadingCheckTable(long size)
        {
            long i = 0;

            List<CheckB> IsCheckTable = new List<CheckB>();

            int plengt = ylengt - 1;
            while (true)
            {
                if (i < size)
                {
                    if ((i + plengt) < size)
                    {
                        IsCheckTable.Add(new CheckB()
                        {
                            Size = ylengt,
                            StartPostion = i,
                            EndPostion = i + plengt
                        });
                    }
                    else if ((i + plengt) == size)
                    {
                        IsCheckTable.Add(new CheckB()
                        {
                            Size = ylengt,
                            StartPostion = i,
                            EndPostion = i + plengt
                        });

                        break;
                    }
                    else
                    {
                        long t = size - i;


                        IsCheckTable.Add(new CheckB()
                        {
                            Size = (int)t,
                            StartPostion = i,
                            EndPostion = i + t - 1
                        });

                        break;

                    }
                }
                else
                {
                    break;
                }

                i += ylengt;

            }

            return IsCheckTable;
        }

        private void 添加用户ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddUser adduser = new AddUser();
            adduser.ShowDialog();

            if (adduser.IsAdd)
            {
                if (UserList == null)
                    UserList = new List<User>();

                if (UserList.Find(p => p.UserName == adduser.UserName) != null)
                {
                    MessageBox.Show("用户名重复");
                    return;
                }

                if (!Directory.Exists(adduser.Path))
                {
                    MessageBox.Show("路径不存在");
                    return;
                }

                User tmp = new User()
                {
                    UserName=adduser.UserName,
                    Password=adduser.PassWord,
                    Path=adduser.Path
                };

                if (UserList == null)
                    UserList = new List<User>();

                UserList.Add(tmp);

                SaveUser();
                LoadingUser();
            }
        }

        private void 删除用户ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in this.listView1.SelectedItems)
                {
                    UserList.RemoveAll(p => p.UserName == item.Text);
                }

                SaveUser();
                LoadingUser();
            }
        }

        private void Server_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Visible = true;
        }
    }
}
