using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.RPCX.Client
{
    public enum LogType
    {
        Log=0,
        Err=1,
        War = 2
    }

    public delegate void LogOutHandler(string msg, LogType type);

    public static class LogAction
    {
        public static event LogOutHandler LogOut;
        

        public static void Warn(string msg, params object[] args)
        {
            if (LogOut != null)
                LogOut(string.Format(msg, args), LogType.War);
        }

        public static void Err(string msg, params object[] args)
        {
            if (LogOut != null)
                LogOut(string.Format(msg, args), LogType.Err);
        }

        public static void Log(string msg, params object[] args)
        {
            if (LogOut != null)
                LogOut(string.Format(msg, args), LogType.Log);
        }

        public static void Log(LogType type,string msg,params object[] args)
        {          
            if (LogOut != null)
                LogOut(string.Format(msg, args), type);
        }
    }
}
