using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ZYSocket.RPC
{
    public class FormatValueType
    {
        static Type objtype ;
        public FormatValueType()
        {
            objtype = typeof(object);
            GetValueMethod = new Dictionary<Type, MethodInfo>();
            GetValueMethod.Add(typeof(object), this.GetType().GetMethod("GetObjectArg", new[] { typeof(object) }));
            GetValueMethod.Add(typeof(int), this.GetType().GetMethod("GetObjectArg", new[] { typeof(int) }));
            GetValueMethod.Add(typeof(short), this.GetType().GetMethod("GetObjectArg", new[] { typeof(short) }));
            GetValueMethod.Add(typeof(long), this.GetType().GetMethod("GetObjectArg", new[] { typeof(long) }));
            GetValueMethod.Add(typeof(float), this.GetType().GetMethod("GetObjectArg", new[] { typeof(float) }));
            GetValueMethod.Add(typeof(double), this.GetType().GetMethod("GetObjectArg", new[] { typeof(double) }));

        }

        public MethodInfo GetMethodInfo(Type type)
        {
            if(type.IsValueType)
                return GetValueMethod[type];
            else
                return GetValueMethod[objtype];
        }

        public  Dictionary<Type, MethodInfo> GetValueMethod
        {
            get; set;
        }

        public static object GetObjectArg(object o)
        {
            return o;
        }

        public static object GetObjectArg(byte o)
        {
            return o;
        }

        public static object GetObjectArg(Int16 o)
        {
            return o;
        }

        public static object GetObjectArg(Int32 o)
        {
            return o;
        }
        public static object GetObjectArg(Int64 o)
        {
            return o;
        }
        public static object GetObjectArg(float o)
        {
            return o;
        }
        public static object GetObjectArg(double o)
        {
            return o;
        }
    }
}
