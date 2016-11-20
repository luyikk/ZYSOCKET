using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace PackHandler
{
    [FormatClassAttibutes((int)PackType.LogOn)]
    public class LogOn
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
}
