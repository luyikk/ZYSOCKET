using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using ZYSocket.share;
using ZYSocket.ClientB;
using ZYSocket.Compression;

namespace Client
{
    public static class SocketManager
    {
        private  static SocketClient client { get; set; }
        public static ZYNetBufferReadStreamV2 Stream { get; set; }
        public static event ClientBinaryInputHandler BinaryInput;
        public static event ClientMessageInputHandler Disconnet;
        private static object Lockobj = new object();
        public static bool IsConnent { get; set; }

        static SocketManager()
        {
            Stream = new ZYNetBufferReadStreamV2(40960);
            client = new SocketClient();
            client.BinaryInput += new ClientBinaryInputHandler(client_BinaryInput);
            client.ErrorLogOut += new ErrorLogOutHandler(client_ErrorLogOut);
            client.MessageInput += new ClientMessageInputHandler(client_MessageInput);
        }

        public static bool Connent(string host, int port)
        {
            if (!IsConnent)
                return (IsConnent = client.Connect(host, port));
            else
                return true;
        }

        public static void StartRead()
        {
            client.StartRead();
        }


        public static void Send(byte[] data)
        {
            client.Send(data);
        }

        static void client_ErrorLogOut(string msg)
        {
            Console.WriteLine(msg);
        }


        static void client_MessageInput(string message)
        {
            IsConnent = false;

            if (Disconnet != null)
                Disconnet(message);
        }

        static void client_BinaryInput(byte[] data)
        {


            try
            {

                if (BinaryInput != null)
                {

                    Stream.Write(data);

                    byte[] datax;
                    while (Stream.Read(out datax))
                    {
                        BinaryInput(datax);
                    }
                }


            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }


        }

    }
}
