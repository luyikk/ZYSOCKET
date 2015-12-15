using System;
using System.Collections.Generic;
using System.Text;

namespace P2PFileInfo
{
    public enum FileCmd
    {
        Success=10000,
        GetFile=10001,

        Down=10002,      
        DownNow = 2000,
        DownClose = 3000,
        ReBytes = 3001,
    }
}
