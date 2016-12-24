using System;
using System.Collections.Generic;
using ZYSocket.share;

namespace ZYSocket.RPCX.Service
{



    [FormatClassAttibutes(1001000)]
    public class RPCCallPack
    {

        public long Id { get; set; }

        public string Tag { get; set; }

        public string Method { get; set; }

        public bool NeedReturn { get; set; }

        public List<byte[]> Arguments { get; set; }

    }

    


    [FormatClassAttibutes(1001001)]
    public class Result_Have_Return
    {
        public long Id { get; set; }

        public byte[] Return { get; set; }

        public List<byte[]> Arguments { get; set; }
    }


}
