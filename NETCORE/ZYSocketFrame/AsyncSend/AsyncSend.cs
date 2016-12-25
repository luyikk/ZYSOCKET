using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using System.Net.Sockets;

namespace ZYSocket.AsyncSend
{
    public class AsyncSend : ISend
    {
        private SocketAsyncEventArgs _send { get; set; }

        private bool SendIng { get; set; }

        private ConcurrentQueue<byte[]> BufferQueue { get; set; }

        private Socket sock { get; set; }

        protected int BufferLenght { get; set; } = -1;


        public AsyncSend(Socket sock)
        {
            this.sock = sock;
            SendIng = false;
            BufferQueue = new ConcurrentQueue<byte[]>();
            _send = new SocketAsyncEventArgs();
            _send.Completed += Completed;
        }

        private void Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    {
                        BeginSend(e);
                    }
                    break;

            }
        }

        private void BeginSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Free();
                return;
            }


            int offset = e.Offset + e.BytesTransferred;

            if (offset < e.Buffer.Length)
            {

                if (BufferLenght > 0)
                {
                    int length = BufferLenght;

                    if (offset + length > e.Buffer.Length)
                        length = e.Buffer.Length - offset;

                    e.SetBuffer(offset, length);
                    sock.SendAsync(_send);
                }
                else
                {
                    e.SetBuffer(offset, e.Count - e.Offset - e.BytesTransferred);
                    sock.SendAsync(_send);
                }
            }
            else
            {
                if (InitData())
                {
                    sock.SendAsync(_send);
                }
                else
                {
                    SendIng = false;
                }
            }

        }

        private void Free()
        {
            _send.SetBuffer(null, 0, 0);

            byte[] tmp;
            for (int i = 0; i < BufferQueue.Count; i++)
                BufferQueue.TryDequeue(out tmp);
        }

        private bool InitData()
        {
            byte[] data;
            if (BufferQueue.TryDequeue(out data))
            {

                if (BufferLenght <= 0)
                {
                    _send.SetBuffer(data, 0, data.Length);

                    return true;
                }
                else
                {
                    int length = BufferLenght;

                    if (length > data.Length)
                        length = data.Length;

                    _send.SetBuffer(data, 0, length);

                    return true;
                }

            }
            else
                return false;
        }


        public bool Send(byte[] data)
        {
            BufferQueue.Enqueue(data);

            if(!SendIng)
            {
                if (InitData())
                {
                    SendIng = true;
                    sock.SendAsync(_send);
                    return true;
                }               
                   
            }

            return false;
        }


        
    }
}
