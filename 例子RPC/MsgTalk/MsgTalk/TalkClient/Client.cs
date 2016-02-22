using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalkClient
{
    [RPCTAG("IClient")]
    public class Client
    {
        [RPCMethod]
        public  void UserTalk(string name, string msg)
        {
            Console.WriteLine(name + " :" + msg);
        }

      
        [RPCMethod]
        public int test()
        {
            return 1;
        }
    }
}
