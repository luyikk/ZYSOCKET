using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.User
{
    //用户数据实现
    class UserData:IUserData
    {
        public bool CheckUser(string userName, string password)
        {
            return true;
        }
    }
}
