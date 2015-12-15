using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;

namespace TestMsgPack
{
    public class A
    {
        [MessagePackMember(0)]
        public string Msg { get; set; }

        [MessagePackMember(1)]
        public int X { get; set; }
    }
}
