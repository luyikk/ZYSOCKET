using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.ZYNet;

namespace DrawBoardServer
{
    public class UserInfo
    {
        public string Name { get; set; }

        public ZYNetSession Session { get; set; }

    }
}
