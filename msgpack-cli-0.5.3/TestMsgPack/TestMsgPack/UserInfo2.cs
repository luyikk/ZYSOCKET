using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
using System.Runtime.Serialization;
using MsgPack;
using ProtoBuf;

namespace TestMsgPack
{
    [ProtoContract]
    public class UserInfo2
    {
        [ProtoMember(1), MessagePackMember(1)]
        public int Age;

        //[ProtoMember(2), MessagePackMember(2)]
        //public int Age1;

        //[ProtoMember(3), MessagePackMember(3)]
        //public int Age2;

        //[ProtoMember(4), MessagePackMember(4)]
        //public int Age3;

        //[ProtoMember(5), MessagePackMember(5)]
        //public int Age4;

        //[ProtoMember(6), MessagePackMember(6)]
        //public string NickName;

        //[ProtoMember(7), MessagePackMember(7)]
        //public string Gender;

        //[ProtoMember(8), MessagePackMember(8)]
        //public string Location;
    }
}