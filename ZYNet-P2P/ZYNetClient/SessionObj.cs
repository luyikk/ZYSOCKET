using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSocket.ZYNet.Client
{
    public class SessionObj
    {
        public long Id { get; set; }

        public bool IsConnect { get; set; }

        internal ConClient Client { get; set; }
    }

}
