using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
using System.Net.Sockets;

namespace Server
{
    public class UserManager
    {
        public string UserName { get; set; }

        public ZYNetRingBufferPoolV2 Stream { get; set; }
        public SocketAsyncEventArgs Asyn { get; set; }
    }
}
