using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace ZYSocket.RPC
{
    public delegate ReturnValue CallHandler(string module, string MethodName, List<Type> argTypelist, List<byte[]> arglist,Type returnType);

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
                List<byte[]> arglist = new List<byte[]>();

                Type[] types = ctorMsg.MethodSignature as Type[];

                List<Type> argsType = new List<Type>(ctorMsg.ArgCount);
              
                object[] args = ctorMsg.Args;

                for (int i = 0; i < ctorMsg.ArgCount; i++)
                {                    
                    argsType.Add(args[i].GetType());
                    arglist.Add(Serialization.PackSingleObject(argsType[i], args[i]));
                }


                ReturnValue returnval = Call(ModuleName, MakeID.MakeMethodName(ctorMsg.MethodName, types), argsType, arglist, (ctorMsg.MethodBase as MethodInfo).ReturnType);

                if (returnval.Args == null)
                {
                    returnval.Args = args;
                }

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
