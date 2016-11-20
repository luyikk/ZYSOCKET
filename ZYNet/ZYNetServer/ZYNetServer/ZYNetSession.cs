using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.share;
namespace ZYSocket.ZYNet
{
    public  class ZYNetSession
    {
        public ZYNetSession(long Id, SocketAsyncEventArgs asyn, ZYNetRingBufferPool bufferQueue)
        {
            this.Id = Id;
            this.Asyn = asyn;
            this.BufferQueue = bufferQueue;
        }

        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// SOCKET ASYN
        /// </summary>
        public SocketAsyncEventArgs Asyn { get; private set; }

        /// <summary>
        /// BUFF 缓冲区
        /// </summary>

        public ZYNetRingBufferPool BufferQueue { get; private set; }

        /// <summary>
        /// 内网IP
        /// </summary>
        public string LANIP { get; set; }


        /// <summary>
        /// 内网下次开放端口
        /// </summary>
        public int NatNextPort { get; set; }

        
        /// <summary>
        /// 外网IP
        /// </summary>

        public string WANIP { get; set; }

        /// <summary>
        /// 外网端口号
        /// </summary>
        public int WANPort { get; set; }


        public int Group { get; set; }



        /// <summary>
        /// USER 自定义数据
        /// </summary>
        public object UserToken { get; set; }



    }
}
