using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    class Program
    {
        class ab
        {
            public ab(string x)
            {
                Name = x;
            }
            public string Name { get; set; }

        }


        static void Main(string[] args)
        {

            var t= typeof(string[]);

            List<object> ad = new List<object>();
            ad.Add("123123");
            ad.Add("4111");

            byte[] a= Serialize(ad);

        }



        public static byte[] Serialize(object pObj)
        {
            using (System.IO.MemoryStream _memory = new System.IO.MemoryStream())
            {
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(_memory, pObj);

                return _memory.ToArray();

            }
        }

        public static T P<T>(byte[] data)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
            {
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        }

    }
}
