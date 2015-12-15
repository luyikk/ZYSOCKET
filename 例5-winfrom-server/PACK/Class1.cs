using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;
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
        public string username { get; set; }
        public string password { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.LogOnRes)]
    public class LOGONRES
    {
        public bool IsLogOn { get; set; }
        public string Msg { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.Data)]
    public class DATA
    {
        public string CMD { get; set; }
    }

    [FormatClassAttibutes((int)PACKTYPE.DataRes)]
    public class DATARES
    {
        public int Type { get; set; }
        public List<string> Res { get; set; }
    }


}
