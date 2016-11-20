using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.RPCX.Client;
using System.Windows.Forms;

namespace RPCTalkClientWin
{
    public static class TalkService
    {
        public static RPCClient Client { get; set; }

        public static Client ClientCall { get; set; }

        static TalkService()
        {
            ClientCall = new RPCTalkClientWin.Client();
        }

        public static bool Connect(string host, int port)
        {
            Client = new RPCClient();
            Client.OutTime = 5000; //超时时间5秒
            Client.Disconn += Client_Disconn;
            Client.RegModule(ClientCall);
            return Client.Connection(host, port);
        }

        private static void Client_Disconn(string message)
        {           
            MessageBox.Show(message);
            Application.Exit();
        }
    }



}
