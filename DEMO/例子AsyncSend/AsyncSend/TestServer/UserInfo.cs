using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.AsyncSend;
using ZYSocket.share;
using System.Net.Sockets;

namespace TestServer
{
    public class UserInfo:AsyncSend
    {
        public ZYNetRingBufferPool Stream { get; set; }

        public SocketAsyncEventArgs Asyn { get; set; }

        public UserInfo(SocketAsyncEventArgs asyn)
            : base(asyn.AcceptSocket)
        {
            Stream = new ZYNetRingBufferPool(1024*1024);
            Asyn = asyn;
            base.BufferLenght = 4096; // <=0 Send All OR for Send value length
        }
        
    }
}
