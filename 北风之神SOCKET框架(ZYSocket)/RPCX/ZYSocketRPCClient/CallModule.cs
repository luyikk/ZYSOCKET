using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace ZYSocket.RPCX.Client
{

    public class ModuleDictionary
    {
        public Dictionary<string, MethodModuleDef> ModuleDiy { get; private set; }


        public ModuleDictionary()
        {
            ModuleDiy = new Dictionary<string, MethodModuleDef>();
        }

        public void Install(object o)
        {
            Type type = o.GetType();
            string tag = Make.GetTypeTag(type);

            var methos = type.GetMethods();

            foreach (var item in methos)
            {
                if (Make.IsCallMethod(item))
                {
                    string methodName = item.Name;
                    var args = item.GetParameters();

                    Type[] typeArg = new Type[args.Length];

                    for (int i = 0; i < args.Length; i++)
                    {
                        typeArg[i] = args[i].ParameterType;
                    }

                    methodName = Make.MakeMethodName(methodName, typeArg);
                    methodName = tag + methodName;


                    if (!ModuleDiy.ContainsKey(methodName))
                        ModuleDiy.Add(methodName, new MethodModuleDef(item,o));
                }
            }
        }

        public MethodModuleDef GetMethod(string tag,string methodName)
        {
            string key = tag + methodName;

            if (ModuleDiy.ContainsKey(key))
                return ModuleDiy[key];
            else
                return null;
        }

    }




    public class MethodModuleDef
    {
        public object Token { get; private set; }


        public bool IsOut { get; set; }

        public MethodInfo methodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        public MethodModuleDef(MethodInfo methodInfo,object token)
        {
            this.Token = token;
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
