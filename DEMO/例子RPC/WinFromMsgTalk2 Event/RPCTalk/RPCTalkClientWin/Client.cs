using System;
using System.Collections.Generic;
using System.Text;

namespace RPCTalkClientWin
{
    public class Client
    {

        public event CALLACTION UpdateUserListEvent;
        [RPCMethod]
        public void UpdateUserList()
        {
            if (UpdateUserListEvent != null)
                UpdateUserListEvent();
        }


        public event CALLACTION<string> MessageShowEvent;
        [RPCMethod]
        public void MessageShow(string msg)
        {
            if (MessageShowEvent != null)
                MessageShowEvent(msg);
        }
    }
}
