#if!COREFX
/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2010-12-26 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace ZYSocket.Server
{
    /// <summary>
    /// 接点读取类
    /// （一个简单的读取appconfig的类)
    /// </summary>
    public static class IPConfig
    {
        /// <summary>
        /// 读取接点到字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public static string ReadString(string key)
        {           
           return ConfigurationManager.AppSettings[key];           
        }

        /// <summary>
        /// 读取一个整数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int ReadInt(string key)
        {
            string val = ReadString(key);

            if (string.IsNullOrEmpty(val))
            {
                throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));                
            }
            else
            {
                int p;

                if (int.TryParse(val, out p))
                {
                    return p;
                }
                else
                {
                    throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));            
                }
            }
        }

    }
}
#endif