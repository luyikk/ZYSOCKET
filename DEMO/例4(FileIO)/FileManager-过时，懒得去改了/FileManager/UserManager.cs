using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ZYSocket.share;
using System.IO;
using System.Collections.Concurrent;

namespace FileManager
{
    public class UserManager
    {

        public ZYNetBufferReadStreamV2 Stream { get; set; }
        public SocketAsyncEventArgs Asyn { get; set; }

        public Dictionary<long, string> DownKeyList { get; set; }

        public ConcurrentDictionary<long, FileStream> StreamList { get; set; }

        public ConcurrentDictionary<long, FileStream> UpFileList { get; set; }

        public ConcurrentDictionary<long,List<CheckB>> IsCheckTable { get; set; }

        public UserManager()
        {
            DownKeyList = new Dictionary<long, string>();
            StreamList = new ConcurrentDictionary<long, FileStream>();
            UpFileList = new ConcurrentDictionary<long, FileStream>();
            IsCheckTable = new ConcurrentDictionary<long, List<CheckB>>();
        }
    }

    public class CheckB
    {
        public int Size { get; set; }
        public long StartPostion { get; set; }
        public long EndPostion { get; set; }
        public bool Checkd { get; set; }
    }
}
