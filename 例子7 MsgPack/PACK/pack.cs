using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
using MsgPack.Serialization;
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


    [FormatClassAttibutes((int)PACKTYPE.LogOn)]
    public class LOGON
    {
        [MessagePackMember(0)]
        public string username { get; set; }
        [MessagePackMember(1)]
        public string password { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.LogOnRes)]
    public class LOGONRES
    {
        [MessagePackMember(0)]
        public bool IsLogOn { get; set; }
        [MessagePackMember(1)]
        public string Msg { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.Data)]
    public class DATA
    {
        [MessagePackMember(0)]
        public string CMD { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.DataRes)]
    public class DATARES
    {
        [MessagePackMember(0)]
        public int Type { get; set; }
        [MessagePackMember(1)]
        public List<string> Res { get; set; }
    }


}
