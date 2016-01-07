using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ZYSocket.RPC
{
    public class ModuleDef
    {
        public Dictionary<string, MethodModuleDef> MethodInfoDiy { get; set; }
        public object Token { get; set; }
        
        public ModuleDef()
        {
            MethodInfoDiy = new Dictionary<string, MethodModuleDef>();
        }

        public void Init(object o)
        {
            Token = o;
            Type type = o.GetType();

            var methos= type.GetMethods();

            foreach (var item in methos)
            {
                string methodName = item.Name;
                var args=item.GetParameters();

                Type[] typeArg = new Type[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    typeArg[i] = args[i].ParameterType;
                }

                methodName = MakeID.MakeMethodName(methodName, typeArg);

                MethodInfoDiy.Add(methodName, new MethodModuleDef(item));
            }
        }
    }

    public class MethodModuleDef
    {
        public MethodInfo methodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        public MethodModuleDef(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;

            var parameters= methodInfo.GetParameters();
            ArgsType = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.Name.LastIndexOf('&')>=0)
                {
                   
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
