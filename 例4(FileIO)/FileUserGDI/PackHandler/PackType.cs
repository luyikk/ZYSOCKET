using System;
using System.Collections.Generic;
using System.Text;

namespace PackHandler
{
    public enum PackType
    {
        LogOn=1000,
        GetDisk=1001,      
        Dir=1002,
        LogOnRes= 10000,
        DelFile=10003,
        NewDir=10004,
        MoveFileSystem=10005,
        Run=10006,
        Down=10007,
        DownNow=2000,
        DownClose=3000,
        ReBytes=3001,

        UpFile=20001,
        UpClose=20002,
        DateUp=20003,
        UpCheck=20004,
        DataSet=20005
    }
}
