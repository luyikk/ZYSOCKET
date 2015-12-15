using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;

namespace Server
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public ZYNetRingBufferPoolV2 Stream { get; set; }
        public System.Net.Sockets.SocketAsyncEventArgs Asyn { get; set; }

        public UserInfo(System.Net.Sockets.SocketAsyncEventArgs asyn)
        {
            this.Asyn = asyn;
            Stream = new ZYNetRingBufferPoolV2(1024 * 1024 * 4);
        }
    }

}
