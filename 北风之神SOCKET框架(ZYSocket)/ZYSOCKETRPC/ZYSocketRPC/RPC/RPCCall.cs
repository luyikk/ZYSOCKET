using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.share;

namespace ZYSocket.RPC
{


    public class AsynReturn
    {
        public object Return { get; set; }
        public Type ReturnType { get; set; }

        public List<Argument> Arguments { get; set; }
        public ZYClient_Result_Return ReturnValue { get; set; }

        public AsynReturn()
        {
            Arguments = new List<Argument>();
        }

        public void Format()
        {
            if (ReturnValue != null)
            {
                ReturnType = ReturnValue.ReturnType;
                Return = Serialization.UnpackSingleObject(ReturnType,ReturnValue.Return);

                if (ReturnValue.Arguments.Count > 0)
                {
                    foreach (var item in ReturnValue.Arguments)
                    {
                        Argument tmp = new Argument()
                        {
                            Type = item.type,
                            value = Serialization.UnpackSingleObject(item.type,item.Value)
                        };
                    }
                }

            }

        }
        public class Argument
        {

            public Type Type { get; set; }
            public object value { get; set; }

        }
    }

    public class RPCArgument
    {
        public Type type { get; set; }

        public Type RefType
        {
            get; set;
        }


        public byte[] Value { get; set; }

    }

    [FormatClassAttibutes(1001000)]
    public class RPCCallPack
    {

        public long Id { get; set; }


        public DateTime CallTime { get; set; }


        public string CallModule { get; set; }

        public string Method { get; set; }


        public List<RPCArgument> Arguments { get; set; }


        public bool IsNeedReturn { get; set; }


    
    }

    [FormatClassAttibutes(1001001)]
    public class ZYClient_Result_Return
    {

        public long Id { get; set; }


        public DateTime CallTime { get; set; }


        public byte[] Return { get; set; }


        public Type ReturnType { get; set; }


        public List<RPCArgument> Arguments { get; set; }
    }
}
