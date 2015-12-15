using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.NewDir)]
    public class NewDir
    {
        public string DirName { get; set; }
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
    }

}
