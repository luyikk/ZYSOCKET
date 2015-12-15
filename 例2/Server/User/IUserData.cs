using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.User
{
    /// <summary>
    /// 用户数据接口
    /// </summary>
    interface IUserData
    {
        bool CheckUser(string userName, string password);
    }
}
