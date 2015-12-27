using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public interface Client
    {
        void UserTalk(string name, string msg);
    }
}
