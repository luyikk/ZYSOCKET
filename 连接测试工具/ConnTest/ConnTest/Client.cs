using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.ClientA;

namespace ConnTest
{
    public class Client
    {
        SocketClient socketclient;

        DateTime time;

        public bool IsConn { get; set; }

        public bool IsRead {

            get
            {
                return DateTime.Now.Subtract(time) < new TimeSpan(0, 0, 30);
            }
        }

       
        string Ip;
        int Port;

        public Client(string ip, int port)
        {
            socketclient = new SocketClient();
            socketclient.Connection += new ConnectionOk(socketclient_Connection);
            socketclient.DataOn += new DataOn(socketclient_DataOn);
            socketclient.Disconnection += new ExceptionDisconnection(socketclient_Disconnection);
            
            this.Ip = ip;
            this.Port = port;
        }

        void socketclient_Disconnection(string message)
        {
            IsConn = false;
        }

        void socketclient_DataOn(byte[] Data)
        {
            time = DateTime.Now;
        }

        public void Connto()
        {
            socketclient.BeginConnectionTo(Ip, Port);
        }

        void socketclient_Connection(string message, bool IsConn)
        {
            this.IsConn = IsConn;
        }

        public void SendData(byte[] data)
        {
            if (IsConn)
            {
                socketclient.SendTo(data);
            }
        }


       
    }
}
