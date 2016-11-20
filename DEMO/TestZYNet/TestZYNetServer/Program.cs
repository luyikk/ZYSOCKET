using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.ZYNet;
using ZYSocket.ZYNet.Server;

namespace TestZYNetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServLOG.LogOuts += LLOG_LogOuts;
            ZYNetServer Server = ZYNetServer.GetInstance();            
            Server.UserDataInput += Server_UserDataInput;
            Server.Start();
            
            Console.ReadLine();
        }

        private static void Server_UserDataInput(ZYNetSession session, byte[] data)
        {
            Console.WriteLine(session.Id + ":" + Encoding.Default.GetString(data));
        }

        private static void LLOG_LogOuts(string msg, EventLogType type)
        {
            Console.WriteLine(msg);
        }
    }
}
