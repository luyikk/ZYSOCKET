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

        public Dictionary<long, AsynRetrunModule> AsynRetrunDiy { get; set; }

        public Dictionary<string, ModuleDef> ModuleDiy { get; set; }


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

            ModuleDiy = new Dictionary<string, ModuleDef>();
            AsynRetrunDiy = new Dictionary<long, AsynRetrunModule>();
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
                    ReturnValue = val
                };

                var callback = AsynRetrunDiy[val.Id];

                asynRet.Format(callback.ReturnType, callback.ArgsType);

                AsynRetrunDiy.Remove(val.Id);

                ThreadPool.QueueUserWorkItem((o) =>
                    {
                        callback.Call((AsynReturn)o);

                    }, asynRet);

                return true;
            }
            else if (ReturnValueDiy.ContainsKey(val.Id))
            {
                WaitReturnValue x = ReturnValueDiy[val.Id];
                ReturnValueDiy.Remove(val.Id);
                x.returnvalue = val;
                x.waitHandle.Set();
                return true;
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

        ReturnValue proxy_Call(string module, string MethodName,List<Type> argTypelist, List<byte[]> arglist,Type returnType)
        {
            ReturnValue tmp = new ReturnValue();

            object[] args;

            object value = CallMethod<object>(module, MethodName, argTypelist, arglist, out args, returnType);

            tmp.returnVal = value;
            tmp.Args = args;

            return tmp;
        }
                        
        public void CallMethod(string module, string MethodName, List<Type> argTypeList, List<byte[]> arglist, out object[] args)
        {
            args = null;

            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = module,
                Method = MethodName,
                Arguments = arglist               
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

                        for (int i = 0; i < argTypeList.Count; i++)
                        {
                            args[i] = Serialization.UnpackSingleObject(argTypeList[i], returnx.Arguments[i]);
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

        public Result CallMethod<Result>(string module, string MethodName, List<Type> argTypeList, List<byte[]> arglist, out object[] args, Type returnType = null)
        {
            args = null;
            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = module,
                Method = MethodName,
                Arguments = arglist              
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

                        for (int i = 0; i < argTypeList.Count; i++)
                        {
                            args[i] = Serialization.UnpackSingleObject(argTypeList[i], returnx.Arguments[i]);
                        }

                    }


                    if (returnx.Return != null)
                    {
                        if (returnType != null)
                        {
                            object returnobj = Serialization.UnpackSingleObject(returnType, returnx.Return);

                            return (Result)returnobj;
                        }
                        else
                        {
                            object returnobj = Serialization.UnpackSingleObject(typeof(Result), returnx.Return);

                            return (Result)returnobj;
                        }
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
            ModuleDef tmp = new ModuleDef();
            tmp.Init(o);
            ModuleDiy.Add(o.GetType().Name,tmp);

        }


        public bool RunModule(RPCCallPack tmp, out object returnValue)
        {
           
            returnValue = null;

            if (ModuleDiy.ContainsKey(tmp.CallModule))
            {
                var module = ModuleDiy[tmp.CallModule];

                if (module.MethodInfoDiy.ContainsKey(tmp.Method))
                {

                    var method = module.MethodInfoDiy[tmp.Method];

                    if (tmp.Arguments != null)
                    {

                        object[] arguments = new object[method.ArgsType.Length];


                        for (int i = 0; i < tmp.Arguments.Count; i++)
                        {
                            arguments[i] = Serialization.UnpackSingleObject(method.ArgsType[i], tmp.Arguments[i]);
                        }


                        returnValue = method.methodInfo.Invoke(module.Token, arguments);

                        if (method.IsOut)
                        {
                            for (int i = 0; i < arguments.Length; i++)
                            {
                                tmp.Arguments[i] = Serialization.PackSingleObject(method.ArgsType[i], arguments[i]);
                            }
                        }
                        else
                            tmp.Arguments = null;

                        return true;

                    }
                    else
                    {
                        returnValue = method.methodInfo.Invoke(module.Token,null);
                        return true;
                    }
                }
                else
                {
                    string msg = "Not find " + tmp.CallModule + "-> public " + tmp.Method;

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
