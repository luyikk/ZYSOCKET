using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.ZYNet.Client;

namespace ZYSocket.ZYNet.PACK
{
    [FormatClassAttibutes(-1000)]
    public class RegSession
    {

        public long Id { get; set; }

        /// <summary>
        /// 本地IP
        /// </summary>
        public string LocalHost { get; set; }

        /// <summary>
        /// 预留字段
        /// </summary>
        public int Group { get; set; }


        /// <summary>
        /// 注册端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }


        public string Msg { get; set; }
    }



    [FormatClassAttibutes(-1001)]
    public class GetAllSession
    {
        /// <summary>
        /// 用户ID列表
        /// </summary>
        public List<long> UserIds { get; set; }

        public bool IsSuccess { get; set; }

        public string Msg { get; set; }
    }


    [FormatClassAttibutes(-1002)]
    public class ConnectTo
    {
        public long Id { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool IsSuccess { get; set; }
    }


    [FormatClassAttibutes(-1003)]
    public class LEFTConnect
    {
        public long Id { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool IsSuccess { get; set; }

    }


    [FormatClassAttibutes(-1004)]
    public class AddSession
    {
        public long Id { get; set; }

    }

    [FormatClassAttibutes(-1005)]
    public class RemoveSession
    {
        public long Id { get; set; }
    }


    [FormatClassAttibutes(-2000)]
    public class ProxyData
    {
        public long Source { get; set; }

        public List<long> Ids { get; set; }

        public byte[] Data { get; set; }
    }

    [FormatClassAttibutes(-3000)]
    public class Client_Data
    {    
        public byte[] Data { get; set; }
    }
}
