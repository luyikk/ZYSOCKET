using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.UpFile)]
    public class UpFile
    {
        public string FullName { get; set; }
        public long Size { get; set; }
        public long UpKey { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }

}
