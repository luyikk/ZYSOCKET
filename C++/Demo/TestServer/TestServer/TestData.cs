using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace TestServer
{
   // [ProtoContract]
    public class TestData
    {
        //[ProtoMember(1)]
        public int Id { get; set; }

       // [ProtoMember(2)]
        public List<string> Data { get; set; }

       // [ProtoMember(3)]
        public List<Test2> Data2 { get; set; }

    }


   // [ProtoContract]
    public class Test2
    {
       // [ProtoMember(1)]
        public int A { get; set; }

       // [ProtoMember(2)]
        public int B { get; set; }
    }

}
