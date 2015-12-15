using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace P2PFileInfo.Pack
{
    [FormatClassAttibutes((int)FileCmd.Success)]
    public class Success
    {
        public bool IsRes { get; set; }
        public string Key { get; set; }
        public bool IsSuccess { get; set; }

    }
}
