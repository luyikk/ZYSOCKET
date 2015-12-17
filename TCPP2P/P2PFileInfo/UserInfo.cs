using System;
using System.Collections.Generic;
using System.Text;
using P2PCLIENT;
using ZYSocket.share;
using System.IO;

namespace P2PFileInfo
{
    public delegate void DownFileDataOn(byte[] data);
    public  class UserInfo
    {
        public ConClient Client { get; set; }

        public ClientInfo MainClient { get; set; }


        public void SendData(byte[] data)
        {
            MainClient.SendData(Client.Key, data);
        }


        public ZYNetRingBufferPoolV2 Stream { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsValidate { get; set; }

        public string ConnentKey { get; set; }

        public DownFileDataOn DownDataOn { get; set; }

        public Dictionary<long, string> DownKeyList { get; set; }
        public Dictionary<long, FileStream> StreamList { get; set; }

        public UserInfo()
        {
            DownKeyList = new Dictionary<long, string>();
            StreamList = new Dictionary<long, FileStream>();
        }

        public override string ToString()
        {
            if (!Client.IsProxy)
            {
                return Client.Host + ":" + Client.Port + (IsSuccess ? "(已登入)" : "") + (IsValidate ? "(已验证)" : "");
            }
            else
            {
                return Client.Key + (IsSuccess ? "(已登入)" : "") + (IsValidate ? "(已验证)" : "");
            }
        }
    }
}
