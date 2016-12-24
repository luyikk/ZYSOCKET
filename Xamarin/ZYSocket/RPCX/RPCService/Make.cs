using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZYSocket.RPCX.Service
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
            tick += R.Next(1, 9999999);
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

            return null;
        }

        public static bool IsCallMethod(MethodInfo method, out string name)
        {
            name = null;

            Attribute[] Attributes = Attribute.GetCustomAttributes(method);

            foreach (Attribute p in Attributes)
            {
                if (p is RPCMethod)
                {
                    name = (p as RPCMethod).Name;

                    return true;
                }
            }

            return false;
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RPCTAG : Attribute
    {
        public string Tag { get; set; }

        public RPCTAG(string tag)
        {
            this.Tag = tag;
        }
    }




    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RPCMethod : Attribute
    {
        public string Name { get; set; }


        public RPCMethod()
        {
            Name = null;
        }

        public RPCMethod(string name)
        {
            Name = name;
        }
    }

}


