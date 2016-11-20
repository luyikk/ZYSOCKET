using UnityEngine;
using System.Collections;
using ZYSocket.RPCX.Client;
public static class NetClient
{
    public static bool IsConnect { get; set; }

    public static RPCClient Net { get; set; }

    public static ClientCall CallBack { get; set; }

    static NetClient()
    {
        CallBack = new ClientCall();
        LogAction.LogOut += LogAction_LogOut;
    }

    private static void LogAction_LogOut(string msg, ZYSocket.RPCX.Client.LogType type)
    {
        Debug.Log(msg);
    }

    public static void Connect()
    {
        Net = new RPCClient();
        IsConnect=Net.Connection("127.0.0.1", 1000);
        if(IsConnect)
        {
          
            Net.Disconn += Net_Disconn;
            Net.RegModule(CallBack);           
        }

    }

    public static Server GetServer()
    {
        if (Net != null && IsConnect)
            return Net.GetRPC<Server>();
        else
            return null;
    }


    private static void Net_Disconn(string message)
    {
        IsConnect = false;
        Debug.Log(message);
    }


}
