using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.ClientB;
using ZYSocket.share;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace ZYSocket.RPC.Client
{
    public delegate void ClientOtherBinaryInputHandler(int cmd, ReadBytes read);

    public delegate void MsgOutHandler(string msg);

    public class RPCClient
    {
        public SocketClient Client { get; set; }
  
        public ZYNetRingBufferPool Stream { get; set; }

        public event ClientOtherBinaryInputHandler DataOn;

        public event ClientMessageInputHandler Disconn;

        public event MsgOutHandler MsgOut;

        public RPC RPC_Call { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get { return RPC_Call.OutTime; } set { RPC_Call.OutTime = value; } }


        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect { get; protected set; }

        public RPCClient()
        {
       
            RPC_Call = new RPC();
            RPC_Call.CallBufferOutSend += RPC_Call_CallBufferOutSend;
            RPC_Call.ErrMsgOut += RPC_Call_ErrMsgOut;
        }

        void RPC_Call_ErrMsgOut(string msg)
        {
            if (MsgOut != null)
                MsgOut(msg);
        }

        public T GetRPC<T>()
        {
            return RPC_Call.GetRPC<T>();
        }
        public void RegModule(object o)
        {
            RPC_Call.RegModule(o);
        }
        private void RPC_Call_CallBufferOutSend(byte[] data)
        {
            if (IsConnect)
            {
                Client.Send(data);
            }
            else
                throw new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.NotConnected);

        }

        public bool Connection(string host, int port)
        {
            if (!IsConnect)
            {

                Stream = new ZYNetRingBufferPool(1024 * 1024); //1M
                Client = new SocketClient();
                Client.BinaryInput += Client_BinaryInput;
                Client.MessageInput += Client_MessageInput;

                if (Client.Connect(host, port))
                {
                    IsConnect = true;
                    Client.StartRead();
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }

        private void Client_MessageInput(string message)
        {
            IsConnect = false;

            if (Disconn != null)
                Disconn(message);
        }



        private void Client_BinaryInput(byte[] data)
        {

            Stream.Write(data);

            byte[] datax;
            while (Stream.Read(out datax))
            {
                BinaryInput(datax);
            }
        }

        public void BinaryInput(byte[] data)
        {
            ReadBytes read = new ReadBytes(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 1001001:
                        {
                            ZYClient_Result_Return val;

                            if (read.ReadObject<ZYClient_Result_Return>(out val))
                            {
                                RPC_Call.SetReturnValue(val);
                            }
                        }
                        break;
                    case 1001000:
                        {
                            ThreadPool.QueueUserWorkItem((o) =>
                                {
                                    RPCCallPack tmp;
                                    ReadBytes pread = (ReadBytes)o;

                                    if (pread.ReadObject<RPCCallPack>(out tmp))
                                    {
                                        object returnValue;

                                        CallContext.SetData("Current", this);

                                        if (RPC_Call.RunModule(tmp, out returnValue))
                                        {

                                            ZYClient_Result_Return var = new ZYClient_Result_Return()
                                            {
                                                Id = tmp.Id,
                                                CallTime = tmp.CallTime,
                                                Arguments = tmp.Arguments
                                            };

                                            if (returnValue != null)
                                            {
                                                var.Return = Serialization.PackSingleObject(returnValue.GetType(), returnValue);
                                            }

                                            Client.BeginSendData(BufferFormat.FormatFCA(var));

                                        }
                                    }
                                }, read);
                        }
                        break;
                    default:
                        {
                            if (DataOn != null)
                                DataOn(cmd, read);
                        }
                        break;
                }


            }

        }
    }
}
