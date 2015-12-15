using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.Run)]
    public class Run
    {
        public string File { get; set; }
        public string Arge { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }
}
