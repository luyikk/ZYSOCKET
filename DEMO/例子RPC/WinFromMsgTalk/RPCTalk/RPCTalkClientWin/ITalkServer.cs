using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCTalkClientWin
{
    interface TalkServer
    {
        bool LogOn(string userName, string passWord);
        List<string> GetAllUser();
        void SendAllMessage(string msg);

        void SendToMessage(string username,string msg);
    }
}
