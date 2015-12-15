using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace P2PFileInfo.Pack
{
    [FormatClassAttibutes((int)FileCmd.Down)]
    public class Down
    {
        public bool IsRes { get; set; }

        public string FullName { get; set; }
        public long Size { get; set; }
        public long DownKey { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }
}
