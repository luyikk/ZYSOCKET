using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ZYSocket.share;

namespace P2PSERVER
{
    public class UserInfo
    {
        public SocketAsyncEventArgs Asyn { get; set; }

       

        /// <summary>
        /// 用户KEY
        /// </summary>
        public string Paw { get; set; }

        /// <summary>
        /// 内网IP
        /// </summary>
        public string LANhost { get; set; }
        /// <summary>
        /// 外网IP
        /// </summary>
        public string WANhost { get; set; }
        /// <summary>
        /// 外网端口号
        /// </summary>
        public string CPort { get; set; }

        /// <summary>
        /// 外网IP地址
        /// </summary>
        public string WANIP { get; set; }

        /// <summary>
        /// 内网端口号
        /// </summary>
        public int NatNetPort { get; set; }

        /// <summary>
        /// 区域区分
        /// </summary>
        public string Mac { get; set; }

        public ZYNetRingBufferPoolV2 BufferQueue { get; set; }
      

        public UserInfo()
        {
           // buffQueue = new ZYNetBufferReadStreamV2(4096);
        }



    }
}
