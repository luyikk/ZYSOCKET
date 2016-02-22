using System;
using System.Collections.Generic;
using System.Text;

namespace RPCConsoleClient
{
  

    public interface ServerClass
    {
        bool LogOn(string username, string password);
        DateTime GetServerTime();
        void OutRandom(out int value);

        Data Return(Data ins);  

        void SendAll(string msg);
        int Add(int a, int b);

        int TestOutAndRef(out int a, ref int b); //注意如果需要 out ref 必须有 return，不然 out 和ref将失效

        string[] array(string[] z);
    }
}
