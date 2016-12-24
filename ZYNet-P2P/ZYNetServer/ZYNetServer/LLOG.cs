using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSocket.ZYNet
{
    public delegate void LogOutHander(string msg, EventLogType type);

    /// <summary>
    /// 日记输出类
    /// </summary>
    public static class ServLOG
    {
        /// <summary>
        /// 日记输出
        /// </summary>
        public static event LogOutHander LogOuts;

        public static void LLOG(string msg, EventLogType type)
        {
            if (LogOuts != null)
                LogOuts(msg, type);
        }
    }

    public enum EventLogType
    {
        None = 0,
        Log = 1,
        ERR = 2,
        INFO = 3
    }
}
