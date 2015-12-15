using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P2PCLIENT
{
    public enum PCMD
    {
        SET = 1001,
        ALLUSER = 1002,
        NOWCONN = 1003,
        LEFTCONN = 1004,


        REGION = 2001,
        GETALLMASK = 2002,
        CONN = 2003,
        GETALLUSER = 2004,
        ProxyData = 2005,

    }
}
