/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com
 *  Updated 2010-12-26 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.share
{
    /// <summary>
    /// 数据包格式化类
    /// （凡是打了此标记的类才能够被 BufferFormat.FormatFCA 处理)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FormatClassAttibutes : Attribute
    {
        public int BufferCmdType { get; set; }

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="bufferCmdType">数据包命令类型</param>
        public FormatClassAttibutes(int bufferCmdType)
        {
            this.BufferCmdType = bufferCmdType;
        }
    }
}
