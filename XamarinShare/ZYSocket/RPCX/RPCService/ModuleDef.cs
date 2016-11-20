using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZYSocket.RPCX.Service
{
    public class ModuleDef
    {
        public Dictionary<string, MethodModuleDef> MethodInfoDiy { get; set; }
        public RPCCallObject Token { get; set; }

        public string ModuleName { get; set; }


        public ModuleDef(RPCCallObject o)
        {
            ModuleName = o.GetType().Name;
            MethodInfoDiy = new Dictionary<string, MethodModuleDef>();
            Init(o);
        }



        private void Init(RPCCallObject o)
        {
            Token = o;
            Type type = o.GetType();

            string tag = Make.GetTypeTag(type);

            if (!string.IsNullOrEmpty(tag))
                ModuleName = tag;


            var methos = type.GetMethods();

            foreach (var item in methos)
            {
                string _methosName;
                if (item.IsPublic)
                {

                    string methodName = item.Name;

                    if (Make.IsCallMethod(item, out _methosName))
                    {
                        if (!string.IsNullOrEmpty(_methosName))
                            methodName = _methosName;
                    }

                    var args = item.GetParameters();

                    Type[] typeArg = new Type[args.Length];

                    for (int i = 0; i < args.Length; i++)
                    {
                        typeArg[i] = args[i].ParameterType;
                    }

                    methodName = Make.MakeMethodName(methodName, typeArg);                

                    if (!MethodInfoDiy.ContainsKey(methodName))
                        MethodInfoDiy.Add(methodName, new MethodModuleDef(item));
                }
            }
        }
    }

    public class MethodModuleDef
    {
        public bool IsOut { get; set; }

        public MethodInfo methodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        public MethodModuleDef(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;

            var parameters = methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.Name.LastIndexOf('&') >= 0)
                {
                    IsOut = true;
                    string type = parameters[i].ParameterType.FullName.Trim(new char[] { '&' });
                    ArgsType[i] = parameters[i].ParameterType.Assembly.GetType(type);
                }
                else
                {
                    ArgsType[i] = parameters[i].ParameterType;
                }
            }
        }
    }
}
