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

namespace ZYSocket.share
{
    /// <summary>
    /// 接点读取类
    /// （一个简单的读取appconfig的类)
    /// </summary>
    public static class RConfig
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


        /// <summary>
        /// 读取一个浮点数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float Readfloat(string key)
        {
            string val = ReadString(key);

            if (string.IsNullOrEmpty(val))
            {
                throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));      
            }
            else
            {
                float p;

                if (float.TryParse(val, out p))
                {
                    return p;
                }
                else
                {
                    throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));      
                }
            }
        }


        /// <summary>
        /// 读取一个长整型
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long ReadfLong(string key)
        {
            string val = ReadString(key);

            if (string.IsNullOrEmpty(val))
            {
                throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));      
            }
            else
            {
                long p;

                if (long.TryParse(val, out p))
                {
                    return p;
                }
                else
                {
                    throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));      
                }
            }
        }


        /// <summary>
        /// 读取一个布尔值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Readfbool(string key)
        {
            string val = ReadString(key);

            if (string.IsNullOrEmpty(val))
            {
                throw new ConfigurationErrorsException(string.Format("节点{0}为空", key));      
            }
            else
            {
                bool p;

                if (bool.TryParse(val, out p))
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
