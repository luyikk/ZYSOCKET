using System;
using System.Collections.Generic;
using System.Text;

namespace P2PCLIENT
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        LogOut.Action += new ActionOutHandler(LogOut_Action);
    //        ClientInfo client = new ClientInfo(Config.Default.ServerIP, Config.Default.ServerPort, Config.Default.ServerRegPort, Config.Default.MinPort, Config.Default.MaxPort,Config.Default.ResCount);
    //        client.ConToServer();
    //        client.ClientDataIn += new ClientDataInHandler(client_ClientDataIn);
    //        client.ClientConnToMe += new ClientConnToHandler(client_ClientConnToMe);
    //        client.ClientDiscon += new ClientDisconHandler(client_ClientDiscon);
    //        while (true)
    //        {

    //            byte[] data = Encoding.Default.GetBytes(Console.ReadLine());

    //            foreach (KeyValuePair<string, ConClient> keyv in client.ConnUserList)
    //            {
    //                keyv.Value.SendData(data);
    //            }


    //        }
    //    }

    //    static void client_ClientDiscon(ConClient client, string message)
    //    {
    //        Console.WriteLine(client.Host + ":" + client.Port + "-" + client.Key + " ->"+message);
    //    }

    //    static void client_ClientConnToMe(ConClient client)
    //    {
    //        Console.WriteLine(client.Host + ":" + client.Port + "-" + client.Key + " 连接");
    //    }

    //    static void client_ClientDataIn(ConClient client, byte[] data)
    //    {
    //        Console.WriteLine(Encoding.Default.GetString(data));
    //    }

    //    static void LogOut_Action(string message, ActionType type)
    //    {
    //        Console.WriteLine(message);
    //    }
    //}
}
