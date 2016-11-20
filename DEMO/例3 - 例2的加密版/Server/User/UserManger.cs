using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.User
{
    //用户数据层操作
    internal static class UserManger
    {
        /// <summary>
        /// 获取用户数据层
        /// </summary>
        /// <returns></returns>
        public static IUserData GetUserDataManger()
        {
            return new UserData();
        }
    }
}
