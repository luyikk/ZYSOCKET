using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace ZYSocket.RPC
{
    public delegate ReturnValue CallHandler(string module, string MethodName, List<RPCArgument> arglist);

    public class ZYProxy : RealProxy
    {
        public string ModuleName { get;  private set; }

        public event CallHandler Call;

        public ZYProxy(Type type)
            : base(type)
        {
            ModuleName = type.Name;

        }

        public override IMessage Invoke(IMessage reqMsg)
        {

            IMethodCallMessage ctorMsg = reqMsg as IMethodCallMessage;

            if (Call != null)
            {
                List<RPCArgument> arglist = new List<RPCArgument>();

                Type[] types = ctorMsg.MethodSignature as Type[];
                object[] args = ctorMsg.Args;

                for (int i = 0; i < ctorMsg.ArgCount; i++)
                {
                    RPCArgument tmp = new RPCArgument();
                    tmp.RefType = types[i];
                    tmp.type = args[i].GetType();
                    tmp.Value = Serialization.PackSingleObject(tmp.type, args[i]);
                    arglist.Add(tmp);
                }

                ReturnValue returnval = Call(ModuleName, ctorMsg.MethodName, arglist);


                return new ReturnMessage(returnval.returnVal, returnval.Args, returnval.Args == null ? 0 : returnval.Args.Length, null, ctorMsg);


            }


            throw new Exception("event not register");

        }


    }



    public class ReturnValue
    {
        public object returnVal { get; set; }

        public object[] Args { get; set; }

    }
}
