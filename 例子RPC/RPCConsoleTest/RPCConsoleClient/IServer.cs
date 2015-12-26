using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPCConsoleClient
{
    public interface ServerClass
    {
        bool LogOn(string username, string password);
        DateTime GetServerTime();
        void OutRandom(out int value);
    }
}
