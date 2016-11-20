using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.ZYNet.Client;
using ZYSocket.ZYNet;
using ZYSocket.share;

namespace TestZYNetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            LogOut.Action += LogOut_Action;
            ZYNetClient client = new ZYNetClient();
           
            client.Connect(RConfig.ReadString("Host"), RConfig.ReadInt("ServicePort"));
            client.DataInput += Client_ClientDataIn;
            client.ConnectToMe += Client_ClientConnToMe;
            client.SessionDisconnect += Client_ClientDiscon;
            client.ServerDisconnect += Client_ServerDisconnect;
          
           
 

            while (true)
            {
                              

                byte[] data = Encoding.Default.GetBytes(Console.ReadLine());

                client.SendDataToALLClient(data);

            }
            
        }

        private static void Client_ServerDisconnect(string message)
        {
            Console.WriteLine("与服务器断开连接");
        }

        private static void Client_ClientDiscon(long Id, string message)
        {
            Console.WriteLine(Id+" ->" + message);
        }

        private static void Client_ClientConnToMe(long Id)
        {
            Console.WriteLine(Id + " 连接");
        }

        private static void Client_ClientDataIn(long Id,byte[] data)
        {
            Console.WriteLine(Id+":"+Encoding.Default.GetString(data));
        }

        private static void LogOut_Action(string message, ActionType type)
        {
            //if(type!=ActionType.None)
                Console.WriteLine(message);
        }
    }
}
