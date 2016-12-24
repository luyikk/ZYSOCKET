using System;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Collections.Generic;

namespace ZYSocket.RPCX.Service
{

    public delegate ProxyReturnValue CallHandler(string Tag, string MethodName, Type[] argTypelist, List<byte[]> arglist, Type returnType);

    public delegate void CallNullHandler(string Tag, string MethodName, Type[] argTypelist, List<byte[]> arglist);



    public class ZYProxy : RealProxy
    {
        public string Tag { get; private set; }

        public event CallHandler CallHaveReturn;
        public event CallNullHandler CallNullReturn;

        private Type nullType = typeof(void);

        public ZYProxy(Type type,bool CallReturn)
           : base(type)
        {
            Tag = type.Name;
            
            if(!CallReturn)
            {
                foreach (var item in type.GetMethods())
                {
                    if (item.ReturnType != typeof(void))
                        throw new Exception("Not call sync method,If you want to use,Set RPCServer.IsCallReturn value is True!");

                    foreach (var parmeters in item.GetParameters())
                    {
                        if (parmeters.ParameterType.Name.LastIndexOf('&') >= 0)
                        {
                            throw new Exception("Not call sync method,If you want to use,Set RPCServer.IsCallReturn value is True!");

                        }
                    }       

                }  
            }            

        }

        public override IMessage Invoke(IMessage reqMsg)
        {
            IMethodCallMessage ctorMsg = reqMsg as IMethodCallMessage;


            List<byte[]> arglist = new List<byte[]>(ctorMsg.ArgCount);

            Type[] types = ctorMsg.MethodSignature as Type[];

            Type[] argsType = new Type[ctorMsg.ArgCount];

            object[] args = ctorMsg.Args;

            for (int i = 0; i < ctorMsg.ArgCount; i++)
            {
                argsType[i] = args[i].GetType();
                arglist.Add(Serialization.PackSingleObject(argsType[i], args[i]));
            }

            var returnType = (ctorMsg.MethodBase as MethodInfo).ReturnType;

            if (returnType != nullType)
            {
                if(CallHaveReturn==null)
                    throw new Exception("event not register");
                ProxyReturnValue returnval = CallHaveReturn(Tag, Make.MakeMethodName(ctorMsg.MethodName, types), argsType, arglist, returnType);
                if (returnval.Args == null)
                {
                    returnval.Args = args;
                }

                return new ReturnMessage(returnval.returnVal, returnval.Args, returnval.Args == null ? 0 : returnval.Args.Length, null, ctorMsg);


            }
            else
            {
                if (CallNullReturn == null)
                    throw new Exception("event not register");

                CallNullReturn(Tag, Make.MakeMethodName(ctorMsg.MethodName, types), argsType, arglist); //如果你没有返回值那么out ref将失效
               

                return new ReturnMessage(null, args, args == null ? 0 : args.Length, null, ctorMsg);
            }

           
        }
    }

    public class ProxyReturnValue
    {
        public object returnVal { get; set; }

        public object[] Args { get; set; }

    }
}
