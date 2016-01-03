using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
using System.Reflection;
using System.Threading;


namespace ZYSocket.RPC
{


    public delegate void CallBufferOutHanlder(byte[] data);

    public delegate void ErrMsgOutHanlder(string msg);

    public class RPC
    {
               

        public FormatValueType FormatValue { get; set; }


        public Dictionary<Type, ZYProxy> ZYProxyDiy { get; set; }


        public Dictionary<long, WaitReturnValue> ReturnValueDiy { get; set; }

        public Dictionary<long, Action<AsynReturn>> AsynRetrunDiy { get; set; }

        public Dictionary<string, object> ModuleDiy { get; set; }


        public event CallBufferOutHanlder CallBufferOutSend;
        /// <summary>
        /// 错误输出
        /// </summary>
        public event ErrMsgOutHanlder ErrMsgOut;

        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }


        public RPC()
        {
            FormatValue = new FormatValueType();
            OutTime = 800;
           
            ModuleDiy = new Dictionary<string, object>();
            AsynRetrunDiy = new Dictionary<long, Action<AsynReturn>>();
            ReturnValueDiy = new Dictionary<long, WaitReturnValue>();
            ZYProxyDiy = new Dictionary<Type, ZYProxy>();
        }
            

        /// <summary>
        /// 设置返回值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetReturnValue(ZYClient_Result_Return val)
        {
            if (AsynRetrunDiy.ContainsKey(val.Id))
            {
                AsynReturn asynRet = new AsynReturn()
                {
                    ReturnValue=val
                };

                asynRet.Format();


                Action<AsynReturn> callback = AsynRetrunDiy[val.Id];
                AsynRetrunDiy.Remove(val.Id);

                ThreadPool.QueueUserWorkItem((o) =>
                    {
                        try
                        {
                            callback((AsynReturn)o);
                        }
                        catch (Exception er)
                        {
                            if (ErrMsgOut != null)
                                ErrMsgOut(er.ToString());
                        }

                       
                    }, asynRet);
            }
            else if (ReturnValueDiy.ContainsKey(val.Id))
            {
                WaitReturnValue x = ReturnValueDiy[val.Id];
                ReturnValueDiy.Remove(val.Id);
                x.returnvalue = val;
                x.waitHandle.Set();
            }
         
          

            return false;
        }



        public T GetRPC<T>()
        {
            Type type = typeof(T);

            if (ZYProxyDiy.ContainsKey(type))
            {
                return (T)ZYProxyDiy[type].GetTransparentProxy();
            }
            else
            {
                ZYProxy proxy = new ZYProxy(type);

                proxy.Call += proxy_Call;

                ZYProxyDiy.Add(type,proxy);

                return (T)proxy.GetTransparentProxy();
            }
        }

        ReturnValue proxy_Call(string module, string MethodName, List<RPCArgument> arglist)
        {
            ReturnValue tmp = new ReturnValue();

            object[] args;

            object value= CallMethod<object>(module, MethodName, arglist, out args);

            tmp.returnVal = value;
            tmp.Args = args;

            return tmp;
        }








        public void CallMethod(string module, string MethodName, List<RPCArgument> arglist, out object[] args)
        {
            args = null;

            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = DateTime.Now,
                CallModule = module,
                Method = MethodName,
                Arguments = arglist,
                IsNeedReturn = true,
            };

            WaitReturnValue var = new WaitReturnValue();

            using (var.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {

                ReturnValueDiy.Add(call.Id, var);

                byte[] data = BufferFormat.FormatFCA(call);

                if (CallBufferOutSend != null)
                    CallBufferOutSend(data);


                if (var.waitHandle.WaitOne(OutTime))
                {
                    ZYClient_Result_Return returnx = var.returnvalue;

                    if (returnx.Arguments != null && returnx.Arguments.Count > 0 && arglist.Count == returnx.Arguments.Count)
                    {
                        args = new object[returnx.Arguments.Count];

                        for (int i = 0; i < returnx.Arguments.Count; i++)
                        {
                            args[i] = Serialization.UnpackSingleObject(returnx.Arguments[i].type, returnx.Arguments[i].Value);
                        }

                    }
                    return;
                }
                else
                {
                    ReturnValueDiy.Remove(call.Id);
                    throw new TimeoutException("out time,Please set the timeout time.");
                }
            }

        }

        public Result CallMethod<Result>(string module, string MethodName, List<RPCArgument> arglist, out object[] args)
        {
            args = null;
            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = DateTime.Now,
                CallModule = module,
                Method = MethodName,
                Arguments = arglist,
                IsNeedReturn = true,
            };



            WaitReturnValue var = new WaitReturnValue();
            using (var.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {


                ReturnValueDiy.Add(call.Id, var);

                byte[] data = BufferFormat.FormatFCA(call);

                if (CallBufferOutSend != null)
                    CallBufferOutSend(data);




                if (var.waitHandle.WaitOne(OutTime))
                {
                    
                    ZYClient_Result_Return returnx = var.returnvalue;

                    Type type = returnx.ReturnType;


                    if (returnx.Arguments != null && returnx.Arguments.Count > 0 && arglist.Count == returnx.Arguments.Count)
                    {
                        args = new object[returnx.Arguments.Count];

                        for (int i = 0; i < returnx.Arguments.Count; i++)
                        {
                            args[i] = Serialization.UnpackSingleObject(returnx.Arguments[i].type, returnx.Arguments[i].Value);
                        }

                    }


                    if (type != null)
                    {
                        object returnobj = Serialization.UnpackSingleObject(type, returnx.Return);

                        return (Result)returnobj;
                    }
                    else
                        return default(Result);

                }
                else
                {


                    ReturnValueDiy.Remove(call.Id);

                    throw new TimeoutException("out time,Please set the timeout time.");
                }
            }

        }


        public void RegModule(object o)
        {
            Type type = o.GetType();

            ModuleDiy.Add(type.Name, o);

        }


        public bool RunModule(RPCCallPack tmp, out object returnValue)
        {
           
            returnValue = null;

            if (ModuleDiy.ContainsKey(tmp.CallModule))
            {
                object o = ModuleDiy[tmp.CallModule];

                Type _type = o.GetType();

                if (tmp.Arguments == null)
                    tmp.Arguments = new List<RPCArgument>();



                object[] arguments = new object[tmp.Arguments.Count];

                Type[] argumentstype = new Type[tmp.Arguments.Count];

                for (int i = 0; i < tmp.Arguments.Count; i++)
                {
                    argumentstype[i] = tmp.Arguments[i].RefType;
                    arguments[i] = Serialization.UnpackSingleObject(tmp.Arguments[i].type,tmp.Arguments[i].Value);
                }


                MethodInfo method = null;

                if (argumentstype.Length > 0)
                    method = _type.GetMethod(tmp.Method, argumentstype);
                else
                    method = _type.GetMethod(tmp.Method);

                if (method != null)
                {
                    returnValue = method.Invoke(o, arguments);

                    for (int i = 0; i < arguments.Length; i++)
                    {
                        tmp.Arguments[i].Value = Serialization.PackSingleObject(arguments[i].GetType(),arguments[i]);
                    }

                    return true;
                }
                else
                {
                    string msg = "Not find " + tmp.CallModule + "-> public " + tmp.Method + "(";
                    int l = 0;
                    foreach (var item in argumentstype)
                    {
                        l++;
                        msg += item.Name;
                        if (l < argumentstype.Length)
                            msg += ",";

                    }
                    msg += ")";

                    if (ErrMsgOut != null)
                        ErrMsgOut(msg);

                    return false;
                }
            }
            else
            {
                string msg = "Not find " + tmp.CallModule;

                if (ErrMsgOut != null)
                    ErrMsgOut(msg);
            }


            return false;
        }

     
    }

    public class WaitReturnValue
    {
        public  ZYClient_Result_Return returnvalue { get; set; }

        public EventWaitHandle waitHandle { get; set; }


    }
}
