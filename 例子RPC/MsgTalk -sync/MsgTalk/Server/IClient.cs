using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public interface IClient
    {
        void UserTalk(string name, string msg);

        string GetID();
    }
}
