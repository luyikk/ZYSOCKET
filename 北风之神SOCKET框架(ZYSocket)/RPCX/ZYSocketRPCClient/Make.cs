using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ZYSocket.RPCX.Client
{

    public static class Make
    {
        public static object _lock = new object();
        public static DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        public static Random R = new Random();

        public static long GetID()
        {
          
            System.DateTime nowTime = System.DateTime.Now;
            long tick = nowTime.Ticks - startTime.Ticks;
            tick = tick * 10000000;
            lock (R)
            {
                tick += R.Next(1, 9999999);
            }
            return tick;

        }

        public static long GetTick()
        {
            System.DateTime nowTime = System.DateTime.Now;
            long tick = nowTime.Ticks - startTime.Ticks;
            return tick;
        }

        public static string MakeMethodName(string method, Type[] typeArg)
        {
            if (typeArg == null)
                return method;

            foreach (var item in typeArg)
            {
                method += "_" + item.Name;
            }

            return method;
        }

        public static string GetTypeTag(Type type)
        {
            Attribute[] Attributes = Attribute.GetCustomAttributes(type);

            foreach (Attribute p in Attributes)
            {
                RPCTAG tag = p as RPCTAG;

                if (tag != null)
                {
                    return tag.Tag;
                }
            }

            return "I"+type.Name;
        }

        public static bool IsCallMethod(MethodInfo method)
        {
            Attribute[] Attributes = Attribute.GetCustomAttributes(method);

            foreach (Attribute p in Attributes)
            {
                if (p is RPCMethod)
                    return true;
            }

            return false;
        }
    }
}
