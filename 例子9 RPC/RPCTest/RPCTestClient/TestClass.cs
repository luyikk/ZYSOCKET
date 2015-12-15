using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPCTest
{
 

    public interface TestUserLogOn
    {
       
        bool IsLogON(string username, out string message);
        List<UserData> GetData();
        string GetMyIP();

    }

    public class UserData
    {
        public int M1 { get; set; }
        public int M2 { get; set; }
        public int M3 { get; set; }
        public int M4 { get; set; }

    }

    public interface ITest
    {
        void Run(string name);
        int GetRes(int i);

        List<string> GetResOP(List<string> re);
    }

    public interface TestTow
    {
        void SetR(int v);

        int GetR();

        DateTime GetTime();

        bool TestRef(ref string a);
        void TestOut(string a, out string b);
    }
}
