using System;
using System.Collections.Generic;
using System.Text;

namespace P2PCLIENT
{
    public delegate void ActionOutHandler(string message,ActionType type);

    public static class LogOut
    {
        public static event ActionOutHandler Action;

        internal static void LogIn(string message, ActionType type)
        {
            if (Action != null)
                Action(message, type);
        }
    }

    [Flags]
    public enum ActionType
    {
        None=0x00,
        Message=0x01,
        Error=0x02,
        ServerDiscon=0x03,
        ServerConn=0x04,
        ServerNotConn=0x05
    }
}
