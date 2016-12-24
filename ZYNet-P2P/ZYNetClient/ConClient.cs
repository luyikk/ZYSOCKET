using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.ZYNet.PACK;

namespace ZYSocket.ZYNet.Client
{
    internal delegate void ConnectionsHandlerFiler(ConClient conClient, bool conn);
    internal delegate void DataOutPutHandlerFiler(long key, ConClient conClient, byte[] Data);
    internal delegate void ExpOutPutHandlerFiler(ConClient conClient, string Message);

    internal class ConClient
    {
        public long Key { get; set; }

        internal SocketClient Sock { get; set; }

        public bool IsProxy { get; set; }

        internal event ConnectionsHandlerFiler Conn;
        public event DataOutPutHandlerFiler DataOutPut;
        internal event ExpOutPutHandlerFiler ExpOUtPut;

        public string Host { get; set; }
        public int Port { get; set; }
               
        public ZYNetRingBufferPool BufferQueue { get; private set; }


        internal ConClient(string host, int port,int maxBuffLength)
        {

            BufferQueue = new ZYNetRingBufferPool(maxBuffLength);
            this.Host = host;
            this.Port = port;
            Sock = new SocketClient();
            Sock.ConnInput += new ConnectionHandler(Connection);
            Sock.BinaryInput += new ClientBinaryInputHandler(DataIn);
            Sock.MessageInput += new ClientMessageInputHandler(ExpInput);

        }

        void ExpInput(string message)
        {
            if (ExpOUtPut != null)
                ExpOUtPut(this, message);
        }

        void DataIn(byte[] data)
        {

            BufferQueue.Write(data);

            byte[] datax;
            while (BufferQueue.Read(out datax))
            {
                if (data != null)
                    DataInFor(data);
            }
           
        }

        void DataInFor(byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int length;
            int cmd;

            if (read.ReadInt32(out length) && read.ReadInt32(out cmd))
            {
                if(cmd==-3000)
                {
                    Client_Data pdata;

                    if (read.ReadObject<Client_Data>(out pdata))
                    {

                        if (DataOutPut != null)
                            DataOutPut(Key, this, pdata.Data);
                    }
                }

            }

         


        }

        public void ConnTo()
        {
            Sock.BeginConnect(Host, Port);
        }


        void Connection(System.Net.Sockets.Socket socket, bool isConn)
        {
            if (Conn != null)
                Conn(this, isConn);


        }

        public void SendData(byte[] data)
        {
            Client_Data tmp = new Client_Data();
            tmp.Data = data;
            Sock.BeginSendData(BufferFormat.FormatFCA(tmp));

        }
    }
}
