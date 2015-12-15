using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Net.Sockets;
using ZYSocket.share;
using MsgPack.Serialization;
using MsgPack;
using ZYSocket.RPC;
using System.Runtime.Remoting.Messaging;

namespace ZYSocket.RPC.Server
{
    public class RPCService
    {



        public ConcurrentDictionary<string, object> ModuleDiy { get; set; }


        public RPCService()
        {
            ModuleDiy = new ConcurrentDictionary<string, object>();
        }

        public void RegModule(object o)
        {
            Type type = o.GetType();

            ModuleDiy.AddOrUpdate(type.FullName, o, (a, b) => o);

        }




        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true 属于次模块,false 不属于此模块数据</returns>
        public bool CallModule(byte[] data, SocketAsyncEventArgs e, out ZYClient_Result_Return returnRes)
        {
            returnRes = null;

            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 1001000:
                        {

                            ZYClientCall tmp;

                            if (read.ReadObject<ZYClientCall>(out tmp))
                            {
                                object returnValue;

                                CallContext.SetData("Current", e);
                             
                                if (RunModule(tmp, out returnValue))
                                {

                                    if (tmp.IsNeedReturn)
                                    {
                                        ZYClient_Result_Return var = new ZYClient_Result_Return()
                                        {
                                            Id = tmp.Id,
                                            CallTime = tmp.CallTime,
                                            Arguments = tmp.Arguments
                                        };

                                        if (returnValue != null)
                                        {
                                            var.Return = MsgPack.Serialization.SerializationContext.Default.GetSerializer(returnValue.GetType()).PackSingleObject(returnValue);
                                            var.ReturnType = returnValue.GetType().FullName;
                                        }

                                        returnRes = var;
                                    }

                                    return true;
                                }
                            }
                        }
                        break;

                }
            }

            return false;

        }

        public void Disconnect(SocketAsyncEventArgs e)
        {
            foreach (var item in ModuleDiy.Values)
            {
                Type type = item.GetType();

                if (type.BaseType == typeof(RPCObject))
                {
                    type.GetMethod("ClientDisconnect").Invoke(item, new[] { e });
                  
                }
                
            }
        }

        protected bool RunModule(ZYClientCall tmp, out object returnValue)
        {
            returnValue = null;

            if (ModuleDiy.ContainsKey(tmp.CallModule))
            {
                object o = ModuleDiy[tmp.CallModule];

                Type _type = o.GetType();

                object[] arguments = new object[tmp.Arguments.Count];

                Type[] argumentstype = new Type[tmp.Arguments.Count];

                for (int i = 0; i < tmp.Arguments.Count; i++)
                {
                    argumentstype[i] = Type.GetType(tmp.ArgumentsType[i]);
                    arguments[i] = MsgPack.Serialization.SerializationContext.Default.GetSerializer(argumentstype[i]).UnpackSingleObject(tmp.Arguments[i]);
                }
                

                var method = _type.GetMethod(tmp.Method, argumentstype);

                if (method != null)
                {
                    returnValue = method.Invoke(o, arguments);

                    return true;
                }
                else
                {
                    var methods = _type.GetMethods();

                    foreach (var item in methods)
                    {
                        if (item.Name == tmp.Method)
                        {
                            var arg = item.GetParameters();

                            if (arg.Length == tmp.ArgumentsType.Count)
                            {
                                bool isCheck = true;
                                for (int i = 0; i < arg.Length; i++)
                                {
                                    if (arg[i].ParameterType.FullName.IndexOf(tmp.ArgumentsType[i]) == -1)
                                    {
                                        isCheck = false;
                                        break;
                                    }
                                }

                                if (isCheck)
                                {
                                    returnValue = item.Invoke(o, arguments);

                                    for (int i = 0; i < arguments.Length; i++)
                                    {
                                        tmp.Arguments[i] = MsgPack.Serialization.SerializationContext.Default.GetSerializer(arguments[i].GetType()).PackSingleObject(arguments[i]);
                                    }

                                    return true;
                                }
                            }
                        }
                    }

                }
            }


            return false;
        }



    }
}
