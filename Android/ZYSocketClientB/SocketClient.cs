/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2011-7-29
 *  .NET  2.0
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace ZYSocket.ClientB
{

    /// <summary>
    /// 数据包输入代理
    /// </summary>
    /// <param name="data">输入包</param>
    /// <param name="socketAsync"></param>
    public delegate void ClientBinaryInputHandler(byte[] data);

    /// <summary>
    /// 异常错误通常是用户断开的代理
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="socketAsync"></param>
    /// <param name="erorr">错误代码</param>
    public delegate void ClientMessageInputHandler(string message);

    public delegate void ErrorLogOutHandler(string msg);

    public delegate void ConnectionHandler(Socket sock, bool conn);

    public class SocketClient
    {

        private Socket sock;
        /// <summary>
        /// Socket对象
        /// </summary>
        public Socket Sock { get { return sock; } }

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int BuffLength { get; set; }

        /// <summary>
        /// 数据输入处理
        /// </summary>
        public event ClientBinaryInputHandler BinaryInput;

        /// <summary>
        /// 异常错误通常是用户断开处理
        /// </summary>
        public event ClientMessageInputHandler MessageInput;

        /// <summary>
        /// 有连接
        /// </summary>
        public event ConnectionHandler ConnInput;


        public event ErrorLogOutHandler ErrorLogOut;

        private SocketError socketError;

        public SocketClient()
        {
           
            BuffLength = 4096;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);


            //add by john at 2012-12-03
            //uint dummy = 0;
            //byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            //BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            //BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            //BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            //sock.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }


        private void ErrorLog(string msg)
        {
            if (ErrorLogOut != null)
                ErrorLogOut(msg);
        }

        /// <summary>
        ///连接到目标主机
        /// </summary>
        /// <param name="host">IP</param>
        /// <param name="prot">端口</param>
        public bool Connect(string host, int port)
        {
            try
            {
                #region ipformat

                IPEndPoint myEnd = null;

                try
                {
                    myEnd = new IPEndPoint(IPAddress.Parse(host), port);
                }
                catch (FormatException)
                {                    
                    foreach (IPAddress s in Dns.GetHostAddresses(host))
                    {
                        if (!s.IsIPv6LinkLocal)
                            myEnd = new IPEndPoint(s, port);
                    }
                }

                #endregion

               

                sock.Connect(myEnd);


                if (sock.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (SocketException er )
            {
                ErrorLog(er.ToString());

                return false;
            }
            catch(Exception ee)
            {
                if (ErrorLogOut == null)
                {
                    throw ee;
                }
                else
                {
                    ErrorLog(ee.ToString());
                    return false;
                }
            }

        }


        /// <summary>
        ///连接到目标主机
        /// </summary>
        /// <param name="host">IP</param>
        /// <param name="prot">端口</param>
        public void BeginConnect(string host, int port)
        {
            try
            {
                #region ipformat

                IPEndPoint myEnd = null;

                try
                {
                    myEnd = new IPEndPoint(IPAddress.Parse(host), port);
                }
                catch (FormatException)
                {
                    IPHostEntry p = Dns.GetHostEntry(Dns.GetHostName());

                    foreach (IPAddress s in p.AddressList)
                    {
                        if (!s.IsIPv6LinkLocal)
                            myEnd = new IPEndPoint(s, port);
                    }
                }

                #endregion


                sock.BeginConnect(myEnd, new AsyncCallback(ConnAsyncCallBack), sock);

            }
            catch(Exception er)
            {
                if (ErrorLogOut == null)
                {
                    throw er;
                }
                else
                {
                    ErrorLog(er.ToString());
                }
            }

        }

        void ConnAsyncCallBack(IAsyncResult result)
        {
            try
            {
                sock.EndConnect(result);

                if (sock.Connected)
                {
                    if (ConnInput != null)
                        ConnInput(sock, true);
                }
                else
                    if (ConnInput != null)
                        ConnInput(sock, false);
            }
            catch (Exception er)
            {
                ErrorLog(er.ToString());

                if (ConnInput != null)
                    ConnInput(sock, false);
            }
        }


        /// <summary>
        /// 开始读取数据
        /// </summary>
        public void StartRead()
        {
            BeginReceive();
        }

        void BeginReceive()
        {
            try
            {
                byte[] data = new byte[BuffLength];

                IAsyncResult reault = sock.BeginReceive(data, 0, data.Length, SocketFlags.None, out socketError, args_Completed, data);


            }
            catch (ObjectDisposedException)
            {              
            
            }
          
        }



        void args_Completed(IAsyncResult reault)
        {
           
                int cout = 0;
                try
                {
                    cout = sock.EndReceive(reault);
                }
                catch (SocketException e)
                {
                    socketError = e.SocketErrorCode;
                }
                catch (ObjectDisposedException)
                {
                    socketError = SocketError.HostDown;
                }
                catch (Exception er)
                {
                    ErrorLog(er.ToString());

                    socketError = SocketError.HostDown;
                }

                if (socketError == SocketError.Success && cout > 0)
                {

                    try
                    {
                        byte[] buffer = reault.AsyncState as byte[];


                        byte[] data = new byte[cout];

                        Array.Copy(buffer, 0, data, 0, data.Length);


                        if (this.BinaryInput != null)
                            this.BinaryInput(data);

                        BeginReceive();

                    }
                    catch (Exception er)
                    {
                        if (ErrorLogOut != null)
                            ErrorLog(er.ToString());                        
                    }

                }
                else
                {

                    try
                    {
                        sock.Close();
                    }
                    catch { }

                    if (MessageInput != null)
                        MessageInput("与服务器连接断开");
                }
            

        }



        public virtual void Send(byte[] data)
        {
            try
            {
                sock.Send(data);
            }
            catch (ObjectDisposedException)
            {
                if (MessageInput != null)
                    MessageInput("sock 对象已释放");
            }
            catch (SocketException)
            {
                try
                {
                    sock.Close();
                }
                catch { }

                if (MessageInput != null)
                    MessageInput("与服务器连接断开");
            }
            catch (Exception er)
            {
                if (ErrorLogOut == null)
                    throw er;
                else
                    ErrorLog(er.ToString());
            }
        }
        
      

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="data"></param>
        public virtual void BeginSendData(byte[] data)
        {
            try
            {
                sock.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, sock);

            }
            catch(Exception er)
            {
                if (ErrorLogOut == null)
                    throw er;
                else
                    ErrorLog(er.ToString());
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
            catch(Exception er)
            {           
                    ErrorLog(er.ToString());
            }
        }


        public void Close()
        {
            try
            {
              
                sock.Close();
              
            }
            catch (Exception er)
            {
                if (ErrorLogOut == null)
                    throw er;
                else
                    ErrorLog(er.ToString());
            }
        }


    }
}
