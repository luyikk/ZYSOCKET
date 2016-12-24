using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZYSocket.RPCX.Client
{
    public class WaitReturnValue
    {
        public Result_Have_Return returnvalue { get; set; }
        public EventWaitHandle waitHandle { get; set; }

        public WaitReturnValue()
        {
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

    }
}
