using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.EnsureSend;
using ZYSocket.share;
using System.Net.Sockets;

namespace TestEnsureSend
{
    public class UserInfo:ZYEnsureSend
    {
        public ZYNetRingBufferPool Stream { get; set; }



        public UserInfo(SocketAsyncEventArgs asyn)
            : base(asyn)
        {
            Stream = new ZYNetRingBufferPool(1024*1024);
        }
        
    }
}
