using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Configuration;

namespace ZYSocket.UDPService
{

   
    /// <summary>
    /// 数据包输入代理
    /// </summary>
    /// <param name="data">输入包</param>
    /// <param name="socketAsync"></param>
    public delegate void BinaryInputHandler(byte[] data,IPEndPoint point, SocketAsyncEventArgs socketAsync);

    public class UDPService : IDisposable
    {
       
           #region 释放
        /// <summary>
        /// 用来确定是否以释放
        /// </summary>
        private bool isDisposed;


        ~UDPService()
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
                    sock.Close();

                    for (int i = 0; i < SocketAsynPool.Count; i++)
                    {
                        SocketAsyncEventArgs args = SocketAsynPool.Pop();

                        BuffManagers.FreeBuffer(args);
                    }


                    ReceiveThread.Abort();
                    
                }
                catch
                {
                }

                isDisposed = true;
            }
        }
        #endregion

        /// <summary>
        /// SOCK对象
        /// </summary>
        private Socket sock;

        /// <summary>
        /// Socket对象
        /// </summary>
        public Socket Sock { get { return sock; } }


        /// <summary>
        /// 数据包管理
        /// </summary>
        private BufferManager BuffManagers;

        /// <summary>
        /// Socket异步对象池
        /// </summary>
        private SocketAsyncEventArgsPool SocketAsynPool;



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
        /// 绑定IP
        /// </summary>
        private string Host;

        /// <summary>
        /// 端口
        /// </summary>
        private int Port;

        private System.Threading.AutoResetEvent[] reset;

        public event BinaryInputHandler BinaryInput;

        public System.Threading.Thread ReceiveThread { get; private set; }

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

        public UDPService(string host,int port, int maxconnectcout, int maxbuffersize)
        {
            this.Host = host;
            this.Port = port;          
            this.MaxBufferSize = maxbuffersize;
            this.MaxConnectCout = maxconnectcout;

            this.reset = new System.Threading.AutoResetEvent[1];
            reset[0] = new System.Threading.AutoResetEvent(false);
           
        }


        public UDPService()
        {
            this.Host = IPConfig.ReadString("Host");  
            this.Port = IPConfig.ReadInt("Port");         
            this.MaxBufferSize = IPConfig.ReadInt("MaxBufferSize");
            this.MaxConnectCout = IPConfig.ReadInt("MaxConnectCout");
 
            this.reset = new System.Threading.AutoResetEvent[1];
            reset[0] = new System.Threading.AutoResetEvent(false);          

        }


        public void Start()
        {
            Run();
        }



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

            if (!Host.Equals("any", StringComparison.CurrentCultureIgnoreCase))
            {
                if (String.IsNullOrEmpty(Host))
                {
                    IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());

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
                        IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());

                        foreach (IPAddress s in p.AddressList)
                        {
                            if (!s.IsIPv6LinkLocal)
                                myEnd = new IPEndPoint(s, Port);
                        }
                    }

                }


            }
          

            sock = new Socket(myEnd.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            sock.DontFragment = true;
            sock.EnableBroadcast = true;            
            SendTimeout = 1000;
            ReceiveTimeout = 1000;                        
            sock.Bind(myEnd);         

            BuffManagers = new BufferManager(MaxConnectCout * MaxBufferSize, MaxBufferSize);
            BuffManagers.Inint();

            SocketAsynPool = new SocketAsyncEventArgsPool(MaxConnectCout);

            for (int i = 0; i < MaxConnectCout; i++)
            {
                SocketAsyncEventArgs socketasyn = new SocketAsyncEventArgs();
                socketasyn.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                socketasyn.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Completed);
                SocketAsynPool.Push(socketasyn);
            }


            reset[0].Set();
            Receive();
        }


        void Receive()
        {
            ReceiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    while (true)
                    {
                        System.Threading.WaitHandle.WaitAll(reset);
                        reset[0].Set();

                        if (SocketAsynPool.Count > 0)
                        {
                            SocketAsyncEventArgs sockasyn = SocketAsynPool.Pop();

                            if (BuffManagers.SetBuffer(sockasyn))
                            {
                                if (!Sock.ReceiveFromAsync(sockasyn))
                                {
                                    BeginReceive(sockasyn);
                                }

                            }
                        }
                        else
                        {
                            reset[0].Reset();
                        }

                    }
                }));

            ReceiveThread.Start();
        }


        void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {                
                case SocketAsyncOperation.ReceiveFrom:
                case SocketAsyncOperation.Receive:
                    BeginReceive(e);
                    break;
            
            }

            e.AcceptSocket = null;
            BuffManagers.FreeBuffer(e);
            SocketAsynPool.Push(e);
            reset[0].Set();
        }

        void BeginReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                byte[] data = new byte[e.BytesTransferred];

                Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, data.Length);

                if (this.BinaryInput != null)
                    this.BinaryInput(data,(IPEndPoint)e.RemoteEndPoint, e);
             
            }         
        }

        public void Send(IPEndPoint ipendpoint, byte[] data)
        {
            sock.BeginSendTo(data, 0, data.Length, SocketFlags.None, ipendpoint, AsynCallBack, sock);
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

    public static class IPConfig
    {
        /// <summary>
        /// 读取接点到字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public static string ReadString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 读取一个整数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int ReadInt(string key)
        {
            string val = ReadString(key);

            if (string.IsNullOrEmpty(val))
            {
                throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));
            }
            else
            {
                int p;

                if (int.TryParse(val, out p))
                {
                    return p;
                }
                else
                {
                    throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));
                }
            }
        }

    }
}
