using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack.Serialization;
using MsgPack;
using ZYSocket.share;

namespace ZYSocket.RPC
{
    [FormatClassAttibutes(1001000)]
    public class ZYClientCall
    {
        [MessagePackMember(0)]
        public string Id { get; set; }

        [MessagePackMember(1)]
        public DateTime CallTime { get; set; }

        [MessagePackMember(2)]
        public string CallModule { get; set; }
        [MessagePackMember(3)]
        public string Method { get; set; }

        [MessagePackMember(4)]
        public List<byte[]> Arguments { get; set; }

        [MessagePackMember(5)]
        public bool IsNeedReturn { get; set; }

        [MessagePackMember(6)]
        public List<string> ArgumentsType { get; set; }


    }

    [FormatClassAttibutes(1001001)]
    public class ZYClient_Result_Return
    {
        [MessagePackMember(0)]
        public string Id { get; set; }

        [MessagePackMember(1)]
        public DateTime CallTime { get; set; }

        [MessagePackMember(2)]
        public byte[] Return { get; set; }

        [MessagePackMember(3)]
        public string ReturnType { get; set; }

        [MessagePackMember(4)]
        public List<byte[]> Arguments { get; set; }
    }
}
