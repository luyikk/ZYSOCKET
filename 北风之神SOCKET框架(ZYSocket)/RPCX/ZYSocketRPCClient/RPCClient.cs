using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
using ZYSocket.ClientB;
using System.Threading;

namespace ZYSocket.RPCX.Client
{
    public delegate void ClientOtherBinaryInputHandler(int cmd, ReadBytes read);

    public class RPCClient
    {
        public SocketClient Client { get; private set; }

        public ZYNetRingBufferPool Stream { get; private set; }

        public event ClientOtherBinaryInputHandler DataOn;

        public event ClientMessageInputHandler Disconn;


        public RPC RPC_Call { get; private set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get { return RPC_Call.OutTime; }   set { RPC_Call.OutTime = value; } }


        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect { get; protected set; }

        public int BuffSize { get; private set; }

        public RPCClient():this(1024*1024)
        {

        }

        public RPCClient(int buffSize)
        {
            BuffSize = buffSize;
            RPC_Call = new RPC();
            RPC_Call.CallBufferOutSend += RPC_Call_CallBufferOutSend;           
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

                Stream = new ZYNetRingBufferPool(BuffSize); //1M
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
                            Result_Have_Return val;

                            if (read.ReadObject<Result_Have_Return>(out val))
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
                                                                      
                                    try
                                    {

                                        if (RPC_Call.RunModule(tmp, tmp.NeedReturn,out returnValue))
                                        {
                                            if (tmp.NeedReturn)
                                            {
                                                Result_Have_Return var = new Result_Have_Return()
                                                {
                                                    Id = tmp.Id,
                                                    Arguments = tmp.Arguments
                                                };

                                                if (returnValue != null)
                                                {
                                                    var.Return = Serialization.PackSingleObject(returnValue.GetType(), returnValue);
                                                }

                                                Client.BeginSendData(BufferFormat.FormatFCA(var));
                                            }

                                        }
                                    }
                                    catch (Exception er)
                                    {
                                        LogAction.Err(er.ToString());
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
