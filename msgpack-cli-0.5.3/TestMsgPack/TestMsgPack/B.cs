using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
using ZYSocket.share;
namespace TestMsgPack
{

    [FormatClassAttibutes(1000)]
    public class B
    {
        [MessagePackMember(0)]
        public string Msg { get; set; }

        [MessagePackMember(1)]
        public int X { get; set; }

        [MessagePackMember(2)]
        public int Res { get; set; }

        [MessagePackMember(3)]
        public DateTime time { get; set; }
    }
}
