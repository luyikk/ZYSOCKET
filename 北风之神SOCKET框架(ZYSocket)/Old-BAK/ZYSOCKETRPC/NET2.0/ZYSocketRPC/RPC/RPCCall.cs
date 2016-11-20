using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace ZYSocket.RPC
{

    public class AsynRetrunModule
    {
        public Type ReturnType { get; set; }

        public List<Type> ArgsType { get; set; }

        public Action<AsynReturn> Call { get; set; }

    }



    public class AsynReturn
    {
        public object Return { get; set; }
     
        public List<byte[]> Arguments { get; set; }
        public ZYClient_Result_Return ReturnValue { get; set; }

        public List<Argument> ArgList { get; set; }

        public AsynReturn()
        {
            Arguments = new List<byte[]>();
        }

        public void Format(Type ReturnType,List<Type> argTypelist)
        {
            if (ReturnValue != null && ReturnValue.Return != null)
            {
                Return = Serialization.UnpackSingleObject(ReturnType, ReturnValue.Return);
            }

            if (ReturnValue.Arguments != null && ReturnValue.Arguments.Count > 0)
            {
                ArgList = new List<Argument>();

                for (int i = 0; i < argTypelist.Count; i++)
                {
                    Argument tmp = new Argument()
                    {
                        Type = argTypelist[i],
                        value = Serialization.UnpackSingleObject(argTypelist[i], ReturnValue.Arguments[i])
                    };

                    ArgList.Add(tmp);
                }
            }

        }
        public class Argument
        {

            public Type Type { get; set; }
            public object value { get; set; }

        }
    }

   
 

    [FormatClassAttibutes(1001000)]
    public class RPCCallPack
    {

        public long Id { get; set; }


        public long CallTime { get; set; }


        public string CallModule { get; set; }


        public string Method { get; set; }


        public List<byte[]> Arguments { get; set; }
              
    
    }

    [FormatClassAttibutes(1001001)]
    public class ZYClient_Result_Return
    {

        public long Id { get; set; }


        public long CallTime { get; set; }



        public byte[] Return { get; set; }

        public List<byte[]> Arguments { get; set; }
    }
}
