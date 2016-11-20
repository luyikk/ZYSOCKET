using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalkClient
{
    [RPCTAG("IClient")]
    public class Client
    {
        [RPCMethod("UserTalk")]
        public  void UserTalk2(string name, string msg)
        {
            Console.WriteLine(name + " :" + msg);
        }

      
        [RPCMethod]
        public void test(int a)
        {
            
        }
    }
}
