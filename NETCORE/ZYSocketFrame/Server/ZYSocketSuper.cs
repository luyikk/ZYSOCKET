/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2014-7-21  此类已支持MONO
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace ZYSocket.Server
{

    /// <summary>
    /// 连接的代理
    /// </summary>
    /// <param name="socketAsync"></param>
    public delegate bool ConnectionFilter(SocketAsyncEventArgs socketAsync);

    /// <summary>
    /// 数据包输入代理
    /// </summary>
    /// <param name="data">输入包</param>
    /// <param name="socketAsync"></param>
    public delegate void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync);


    /// <summary>
    /// 数据包输入代理，可少一次COPY
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="socketAsync"></param>
    public delegate void BinaryInputOffsetHandler(byte[] data, int offset, int count, SocketAsyncEventArgs socketAsync);

    /// <summary>
    /// 异常错误通常是用户断开的代理
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="socketAsync"></param>
    /// <param name="erorr">错误代码</param>
    public delegate void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr);

    /// <summary>
    /// ZYSOCKET框架 服务器端
    ///（通过6W个连接测试。理论上支持10W个连接，可谓.NET最强SOCKET模型）
    /// </summary>
    public class ZYSocketSuper : IDisposable
    {

        #region 释放
        /// <summary>
        /// 用来确定是否以释放
        /// </summary>
        private bool isDisposed;


        ~ZYSocketSuper()
        {
            this.Dispose(false);

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed||disposing)
            {
                try
                {
                    sock.Shutdown(SocketShutdown.Both);
#if !COREFX
                    sock.Close();
#endif
                    for (int i = 0; i < SocketAsynPool.Count; i++)
                    {
                        SocketAsyncEventArgs args = SocketAsynPool.Pop();

                        BuffManagers.FreeBuffer(args);
                    }

                  
                }
                catch
                {
                }

                isDisposed = true;
            }
        }
#endregion

        /// <summary>
        /// 数据包管理
        /// </summary>
        private BufferManager BuffManagers;

        /// <summary>
        /// Socket异步对象池
        /// </summary>
        private SocketAsyncEventArgsPool SocketAsynPool;

        /// <summary>
        /// SOCK对象
        /// </summary>
        private Socket sock;

        /// <summary>
        /// Socket对象
        /// </summary>
        public Socket Sock { get { return sock; } }


        /// <summary>
        /// 连接传入处理
        /// </summary>
        public ConnectionFilter Connetions { get; set; }

        /// <summary>
        /// 数据输入处理
        /// </summary>
        public BinaryInputHandler BinaryInput { get; set; }

        /// <summary>
        /// 数据包输入带Offset
        /// </summary>
        public BinaryInputOffsetHandler BinaryOffsetInput { get; set; }
        /// <summary>
        /// 异常错误通常是用户断开处理
        /// </summary>
        public MessageInputHandler MessageInput { get; set; }


        private System.Threading.AutoResetEvent[] reset;

        /// <summary>
        /// 是否关闭SOCKET Delay算法
        /// </summary>
        public bool NoDelay
        {
            get
            {
                return sock.NoDelay;
            }

            set
            {
                sock.NoDelay = value;
            }

        }

        /// <summary>
        /// SOCKET 的  ReceiveTimeout属性
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return sock.ReceiveTimeout;
            }

            set
            {
                sock.ReceiveTimeout = value;

            }


        }

        /// <summary>
        /// SOCKET 的 SendTimeout
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return sock.SendTimeout;
            }

            set
            {
                sock.SendTimeout = value;
            }

        }


        /// <summary>
        /// 接收包大小
        /// </summary>
        private int MaxBufferSize;

        public int GetMaxBufferSize
        {
            get
            {
                return MaxBufferSize;
            }
        }

        /// <summary>
        /// 最大用户连接
        /// </summary>
        private int MaxConnectCout;

        /// <summary>
        /// 最大用户连接数
        /// </summary>
        public int GetMaxUserConnect
        {
            get
            {
                return MaxConnectCout;
            }
        }

        /// <summary>
        /// 是否使用带offset和count的输出,ture的话输入包输入直接调用
        /// </summary>
        public bool IsOffsetInput { get; set; }




        /// <summary>
        /// IP
        /// </summary>
        private string Host;

        /// <summary>
        /// 端口
        /// </summary>
        private int Port;



#region 消息输出
        /// <summary>
        /// 输出消息
        /// </summary>
        public event EventHandler<LogOutEventArgs> MessageOut;


        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="o"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        protected void LogOutEvent(Object sender, LogType type, string message)
        {
            if (MessageOut != null)
                MessageOut.BeginInvoke(sender, new LogOutEventArgs(type, message), new AsyncCallback(CallBackEvent), MessageOut);

        }
        /// <summary>
        /// 事件处理完的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void CallBackEvent(IAsyncResult ar)
        {
            EventHandler<LogOutEventArgs> MessageOut = ar.AsyncState as EventHandler<LogOutEventArgs>;
            if (MessageOut != null)
                MessageOut.EndInvoke(ar);
        }
#endregion

       

        public ZYSocketSuper(string host, int port, int maxconnectcout, int maxbuffersize)
        {
            this.Port = port;
            this.Host = host;
            this.MaxBufferSize = maxbuffersize;
            this.MaxConnectCout = maxconnectcout;

           
            this.reset = new System.Threading.AutoResetEvent[1];
            reset[0] = new System.Threading.AutoResetEvent(false);

            Run();

        }

 #if !COREFX
        public ZYSocketSuper()
        {


            this.Port = IPConfig.ReadInt("Port");
            this.Host = IPConfig.ReadString("Host");
            this.MaxBufferSize = IPConfig.ReadInt("MaxBufferSize");
            this.MaxConnectCout = IPConfig.ReadInt("MaxConnectCout");

            

            this.reset = new System.Threading.AutoResetEvent[1];
            reset[0] = new System.Threading.AutoResetEvent(false);

            Run();

        }
#endif
        /// <summary>
        /// 启动
        /// </summary>
        private void Run()
        {
            if (isDisposed == true)
            {
                throw new ObjectDisposedException("ZYServer is Disposed");
            }


            IPEndPoint myEnd = new IPEndPoint(IPAddress.Any, Port);

            if (!Host.Equals("any",StringComparison.CurrentCultureIgnoreCase))
            {
                if (String.IsNullOrEmpty(Host))
                {
#if !COREFX
                    IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
#else
                    IPHostEntry p = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
#endif

                    foreach (IPAddress s in p.AddressList)
                    {
                        if (!s.IsIPv6LinkLocal && s.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            myEnd = new IPEndPoint(s, Port);
                            break;
                        }
                    }
                  
                }
                else
                {
                    try
                    {
                        myEnd = new IPEndPoint(IPAddress.Parse(Host), Port);
                    }
                    catch (FormatException)
                    {
#if !COREFX
                        IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());
#else
                        IPHostEntry p = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
#endif
                        foreach (IPAddress s in p.AddressList)
                        {
                            if (!s.IsIPv6LinkLocal)
                                myEnd = new IPEndPoint(s, Port);
                        }
                    }

                }
            
            
            }

            sock = new Socket(myEnd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);


 #if !COREFX
            if (Environment.OSVersion.Platform.ToString().IndexOf("NT") >= 0) //WINDOWS NT平台
            {


                //add by john at 2012-12-03 用户检测用户断开 心跳处理
                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                sock.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

                //-------------------------
            }
#endif

            sock.Bind(myEnd);
            sock.Listen(20);
            SendTimeout = 1000;
            ReceiveTimeout = 1000;

            BuffManagers = new BufferManager(MaxConnectCout * MaxBufferSize, MaxBufferSize);
            BuffManagers.Inint();

            SocketAsynPool = new SocketAsyncEventArgsPool(MaxConnectCout);

            for (int i = 0; i < MaxConnectCout; i++)
            {
                SocketAsyncEventArgs socketasyn = new SocketAsyncEventArgs();
                socketasyn.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Completed);
                SocketAsynPool.Push(socketasyn);
            }

           

            Accept();
        }

        public void Start()
        {
            reset[0].Set();
           
        }

        public void Stop()
        {
            reset[0].Reset();
        }

        void Accept()
        {
           
            
            if (SocketAsynPool.Count > 0)
            {
                SocketAsyncEventArgs sockasyn = SocketAsynPool.Pop();
                if (!Sock.AcceptAsync(sockasyn))
                {
                    BeginAccep(sockasyn);
                }
            }
            else
            {
                LogOutEvent(null, LogType.Error, "The MaxUserCout");
            }
        }

        void BeginAccep(SocketAsyncEventArgs e)
        {
            try
            {
                

                if (e.SocketError == SocketError.Success)
                {

                    System.Threading.WaitHandle.WaitAll(reset);
                    reset[0].Set();

                    if (this.Connetions != null)
                        if (!this.Connetions(e))
                        {

                            LogOutEvent(null, LogType.Error, string.Format("The Socket Not Connect {0}", e.AcceptSocket.RemoteEndPoint));
                            e.AcceptSocket = null;
                            SocketAsynPool.Push(e);

                            return;
                        }

                   

                    if (BuffManagers.SetBuffer(e))
                    {
                        if (!e.AcceptSocket.ReceiveAsync(e))
                        {
                            BeginReceive(e);
                        }
                       
                    }

                }
                else
                {
                    e.AcceptSocket = null;
                    SocketAsynPool.Push(e);                 
                    LogOutEvent(null, LogType.Error, "Not Accep");
                }
            }
            finally
            {
                Accept();
            }

        }


      

        void BeginReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success&&e.BytesTransferred>0)
            {

                if (!IsOffsetInput)
                {
                    byte[] data = new byte[e.BytesTransferred];

                    Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, data.Length);

                    if (this.BinaryInput != null)
                        this.BinaryInput(data, e);
                }
                else
                {
                    if (this.BinaryOffsetInput != null)
                        this.BinaryOffsetInput(e.Buffer,e.Offset,e.BytesTransferred,e);

                }
                              
                if (!e.AcceptSocket.ReceiveAsync(e))
                {
                    BeginReceive(e);
                }
            }
            else
            {
                if (e.AcceptSocket != null && e.AcceptSocket.RemoteEndPoint != null)
                {

                    string message;

                    try
                    {
                        message = string.Format("User Disconnect :{0}", e.AcceptSocket.RemoteEndPoint.ToString());
                    }
                    catch (System.NullReferenceException)
                    {
                        message = "User Disconect";
                    }


                    LogOutEvent(null, LogType.Error, message);

                    if (MessageInput != null)
                    {
                        MessageInput(message, e, 0);
                    }

                }
                else
                {
                    if (MessageInput != null)
                    {
                        MessageInput("User Disconnect But cannot get Ipaddress", e, 0);
                    }
                }

                e.AcceptSocket = null;
                BuffManagers.FreeBuffer(e);
                SocketAsynPool.Push(e);
                if (SocketAsynPool.Count == 1)
                {
                    Accept();
                }
            }

        }

     

        void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    BeginAccep(e);
                    break;
                case SocketAsyncOperation.Receive:
                    BeginReceive(e);
                    break;
               
            }
            
        }


        public virtual void Send(ISend player, byte[] data)
        {
            player.Send(data);
        }


        /// <summary>
        /// 同步发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="Data"></param>
        public virtual void Send(Socket sock, byte[] Data)
        {
            try
            {
                if (sock != null && sock.Connected)
                    sock.Send(Data);
            }
            catch (SocketException)
            {

            }

        }

#if !COREFX
        /// <summary>
        /// 异步发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="data"></param>
        public virtual void SendData(Socket sock, byte[] data)
        {
            try
            {
                if (sock != null && sock.Connected)
                    sock.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, sock);
            }
            catch(SocketException)
            {

            }
        }

        void AsynCallBack(IAsyncResult result)
        {
            try
            {
                Socket sock = result.AsyncState as Socket;

                if (sock != null)
                {
                    sock.EndSend(result);
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 断开此SOCKET
        /// </summary>
        /// <param name="sock"></param>
        public void Disconnect(Socket socks)
        {
            try
            {
                if (sock != null)
                    socks.BeginDisconnect(false, AsynCallBackDisconnect, socks);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {

            }

        }

        void AsynCallBackDisconnect(IAsyncResult result)
        {
            try
            {
                Socket sock = result.AsyncState as Socket;

                if (sock != null)
                {
                    try
                    {
                        sock.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        sock.EndDisconnect(result);
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception)
            {

            }
        }
#else


        



        /// <summary>
        /// 断开此SOCKET
        /// </summary>
        /// <param name="sock"></param>
        public void Disconnect(Socket socks)
        {
            try
            {
                socks.Shutdown(SocketShutdown.Both);
              

            }
            catch (Exception)
            {

            }

        }

#endif




    }

    public enum LogType
    {
        Error,
    }


    public class LogOutEventArgs : EventArgs
    {

        /// <summary>
        /// 消息类型
        /// </summary>     
        private LogType messClass;

        /// <summary>
        /// 消息类型
        /// </summary>  
        public LogType MessClass
        {
            get { return messClass; }
        }



        /// <summary>
        /// 消息
        /// </summary>
        private string mess;

        public string Mess
        {
            get { return mess; }
        }

        public LogOutEventArgs(LogType messclass, string str)
        {
            messClass = messclass;
            mess = str;

        }


    }
}
