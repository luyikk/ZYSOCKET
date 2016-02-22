using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPCConsoleTest
{
    public interface IClientCall
    {
        DateTime GetClientDateTime();
        long Add(long a, long b);
        int RecComputer(int i);
        float RecComputer2(float i);

        void ShowMsg(string msg);
    }
}
