using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.RPC.Client;

namespace TalkClient
{
    public class Client:RPCClientObj
    {
        void UserTalk(string name, string msg)
        {
            Console.WriteLine(name + " :" + msg);
        }

      
    }
}
