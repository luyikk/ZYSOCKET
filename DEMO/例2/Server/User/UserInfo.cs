using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;

namespace Server.User
{
    //用户对象
    public class UserInfo
    {
        public string UserName { get; set; }
        public ZYNetRingBufferPool BuffManger { get; set; }

        public UserInfo()
        {
            BuffManger = new ZYNetRingBufferPool();
        }
    }
}
