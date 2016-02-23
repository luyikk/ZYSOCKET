using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.RPCX.Service;

namespace MyNetServer
{
    public class Server:RPCCallObject
    {
        public void UpdateCurrentRotation(float x, float y, float z)
        {
            GetCurrentRPCUser().GetRPC<IClient>().SetPostion(x, y, z);

          //  Console.WriteLine("Rotation x:{0},y:{1},z:{2}", x, y, z);
        }
    }
}
