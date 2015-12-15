using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
using ProtoBuf;

namespace PACK
{
    [Flags]
    public enum PACKTYPE
    {
        LogOn=1001,
        LogOnRes=1002,
        Data=1003,
        DataRes=1004
    }


    [ProtoContract]
    [FormatClassAttibutes((int)PACKTYPE.LogOn)]
    public class LOGON
    {
        [ProtoMember(1)]
        public string username { get; set; }
        [ProtoMember(2)]
        public string password { get; set; }
    }

    [ProtoContract]
    [FormatClassAttibutes((int)PACKTYPE.LogOnRes)]
    public class LOGONRES
    {
         [ProtoMember(1)]
        public bool IsLogOn { get; set; }
         [ProtoMember(2)]
        public string Msg { get; set; }
    }

    [ProtoContract]
    [FormatClassAttibutes((int)PACKTYPE.Data)]
    public class DATA
    {
        [ProtoMember(1)]
        public string CMD { get; set; }
    }

    [ProtoContract]
    [FormatClassAttibutes((int)PACKTYPE.DataRes)]
    public class DATARES
    {
        [ProtoMember(1)]
        public int Type { get; set; }
        [ProtoMember(2)]
        public List<string> Res { get; set; }
    }


}
