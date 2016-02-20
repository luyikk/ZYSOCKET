using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.RPC.Client;
using System.Windows.Forms;

namespace RPCTalkClientWin
{
    public static class TalkService
    {
        public static RPCClient Client { get; set; }

        static TalkService()
        {
           
        }

        public static bool Connect(string host, int port)
        {
            Client = new RPCClient();
            Client.OutTime = 5000; //超时时间5秒
            Client.Disconn += Client_Disconn;
            return Client.Connection(host, port);
        }

        private static void Client_Disconn(string message)
        {           
            MessageBox.Show(message);
            Application.Exit();
        }
    }
}
