using System;
using System.Collections.Generic;
using System.Text;

namespace PackHandler
{
    [ZYSocket.share.FormatClassAttibutes((int)PackType.LogOnRes)]
    public class LogOnRes
    {
        public bool IsOk { get; set; }

        public string Msg { get; set; }

    }
}
