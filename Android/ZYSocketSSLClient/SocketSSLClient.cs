using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ZYSocket.SSLClientB
{

    public delegate X509CertificateCollection GetCertHandler();

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

    public class SocketSSLClient
    {

        public string CertPath { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }

        public TcpClient TcpClient { get; set; }

        public int BuffLength = 4096;
        public SocketError socketError { get; set; }

        public SslStream SslStreamx { get; set; }

        public event ErrorLogOutHandler ErrorLog;

        public event GetCertHandler GetCerts;

        public string SSLHost { get; set; }

        /// <summary>
        /// 数据输入处理
        /// </summary>
        public event ClientBinaryInputHandler BinaryInput;

        /// <summary>
        /// 异常错误通常是用户断开处理
        /// </summary>
        public event ClientMessageInputHandler MessageInput;


        public bool ValidateServerCertificate(
                  object sender,
                  X509Certificate certificate,
                  X509Chain chain,
                  SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (ErrorLog != null)
                ErrorLog("Certificate error: " + sslPolicyErrors);

            return true;
        }

        public SocketSSLClient(string certPath)
        {
            TcpClient = new TcpClient();
            socketError = SocketError.Success;
            CertPath = certPath;
        }

        public SocketSSLClient()
        {
            TcpClient = new TcpClient();
            socketError = SocketError.Success;
        }


        public bool Connect(string host, int port)
        {
            Host = host;
            Port = port;


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


            try
            {
                TcpClient.Connect(myEnd.Address, Port);

            }
            catch (Exception er)
            {
                if (ErrorLog != null)
                    ErrorLog(er.Message);

                return false;
            }


            SslStreamx = new SslStream(TcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            X509CertificateCollection certs = null;

            if (CertPath != null)
            {

                certs = new X509CertificateCollection();
                X509Certificate cert = X509Certificate.CreateFromCertFile(CertPath);
                certs.Add(cert);
            }
            else
            {
                certs = GetCerts();
            }

            try
            {
                if (SSLHost == null)
                    SslStreamx.AuthenticateAsClient(Host, certs, SslProtocols.Tls, false);
                else
                    SslStreamx.AuthenticateAsClient(SSLHost, certs, SslProtocols.Tls, false);
            }
            catch (AuthenticationException e)
            {
                if (ErrorLog != null)
                    ErrorLog("Exception:" + e.Message);


                if (e.InnerException != null)
                {
                    if (ErrorLog != null)
                        ErrorLog("Inner exception:" + e.InnerException.Message);
                }

                TcpClient.Close();
                return false;
            }

            return true;

        }

        public void StartRead()
        {
            System.Threading.ThreadPool.QueueUserWorkItem((a) =>
            {
                BeginReceive();
            });
        }


        void BeginReceive()
        {
            try
            {
                byte[] data = new byte[BuffLength];

                IAsyncResult reault = SslStreamx.BeginRead(data, 0, data.Length, args_Completed, data);


            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception er)
            {
                ErrorLog(er.ToString());

                socketError = SocketError.HostDown;
            }

        }

        void args_Completed(IAsyncResult reault)
        {

            int cout = 0;
            try
            {
                cout = SslStreamx.EndRead(reault);
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
                    if (ErrorLog != null)
                        ErrorLog(er.ToString());
                }

            }
            else
            {

                try
                {
                    TcpClient.Close();
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
                SslStreamx.Write(data);
                SslStreamx.Flush();
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
                    TcpClient.Close();
                }
                catch { }

                if (MessageInput != null)
                    MessageInput("与服务器连接断开");
            }
            catch (Exception er)
            {
                if (ErrorLog == null)
                    throw er;
                else
                    ErrorLog(er.ToString());
            }
        }


        public void Close()
        {
            try
            {
                TcpClient.Close();

            }
            catch (Exception er)
            {
                if (ErrorLog == null)
                    throw er;
                else
                    ErrorLog(er.ToString());
            }
        }



    }
}
