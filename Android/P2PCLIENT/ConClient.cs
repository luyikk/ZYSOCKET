using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.ClientB;
using System.Net.Sockets;

namespace P2PCLIENT
{
    internal delegate void ConnectionsHandlerFiler(ConClient conClient,bool conn);
    public delegate void DataOutPutHandlerFiler(string key,ConClient conClient, byte[] Data);
    internal delegate void ExpOutPutHandlerFiler(ConClient conClient, string Message);

    public class ConClient
    {
        public string Key { get; set; }

        public SocketClient Sock { get; set; }

        public bool IsProxy { get; set; }

        internal event ConnectionsHandlerFiler Conn;
        public event DataOutPutHandlerFiler DataOutPut;
        internal event ExpOutPutHandlerFiler ExpOUtPut;

        public string Host { get; set; }
        public int Port { get; set; }

        public object UserToken { get; set; }

        internal ConClient(string key)
        {
            IsProxy = true;
            this.Key = key;
        }

      

        internal ConClient(string host, int port)
        {
            
            this.Host = host;
            this.Port = port;
            Sock = new SocketClient();
            Sock.ConnInput += new ZYSocket.ClientB.ConnectionHandler(Connection);
            Sock.BinaryInput += new ZYSocket.ClientB.ClientBinaryInputHandler(DataIn);
            Sock.MessageInput += new ZYSocket.ClientB.ClientMessageInputHandler(ExpInput);

        }

        void ExpInput(string message)
        {
            if (ExpOUtPut != null)
                ExpOUtPut(this, message);
        }

        void DataIn(byte[] data)
        {           
            if (data != null)
                DataInFor(data);
        }

        void DataInFor(byte[] data)
        {
            if (DataOutPut != null)
                DataOutPut(Key, this, data);
        }

        public void ConnTo()
        {
            Sock.BeginConnect(Host, Port);
        }


        void Connection(Socket sock,bool isConn)
        {
            if (Conn != null)
                Conn(this, isConn);

        }

        internal void SendData(byte[] data)
        {
            Sock.Send(data);
        }
    }
}
