using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

namespace ZYSocket.RPC
{
    public class RPCObject
    {
        protected SocketAsyncEventArgs GetCurrentSocketAsynEvent()
        {
            return CallContext.GetData("Current") as SocketAsyncEventArgs;
        }

        protected virtual void Send(SocketAsyncEventArgs socketasyn, byte[] data)
        {        

            if (socketasyn != null)
            {
                Socket sock = socketasyn.AcceptSocket;

                try
                {
                    if (sock != null && sock.Connected)
                        sock.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, sock);
                }
                catch (SocketException)
                {

                }
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

        public virtual void ClientDisconnect(SocketAsyncEventArgs socketasyn)
        {
            return;
        }
    }
}
