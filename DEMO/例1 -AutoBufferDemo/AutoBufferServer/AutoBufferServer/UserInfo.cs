using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;

namespace AutoBufferServer
{
    public class UserInfo
    {
        public string UserName { get; set; }

        public string PassWord { get; set; }

        public SocketAsyncEventArgs Asyn { get; set; }

        public ZYNetRingBufferPool BuffPool { get; set; }

    }
}
