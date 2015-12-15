using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public interface TalkService
    {
        bool LogOn(string nickname, out string message);
        List<string> GetAllUser();
        bool MessageTalk(string nick, string context);
    }
}
