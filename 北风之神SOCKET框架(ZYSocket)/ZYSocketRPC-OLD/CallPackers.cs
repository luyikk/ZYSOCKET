using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;

namespace ZYSocket.RPC
{
    [FormatClassAttibutes(1001000)]
    public class ZYClientCall
    {
     
        public string Id { get; set; }

       
        public DateTime CallTime { get; set; }

      
        public string CallModule { get; set; }
      
        public string Method { get; set; }

       
        public List<byte[]> Arguments { get; set; }

      
        public bool IsNeedReturn { get; set; }

      
        public List<string> ArgumentsType { get; set; }


    }

    [FormatClassAttibutes(1001001)]
    public class ZYClient_Result_Return
    {
      
        public string Id { get; set; }

       
        public DateTime CallTime { get; set; }

     
        public byte[] Return { get; set; }

      
        public string ReturnType { get; set; }

      
        public List<byte[]> Arguments { get; set; }
    }
}
