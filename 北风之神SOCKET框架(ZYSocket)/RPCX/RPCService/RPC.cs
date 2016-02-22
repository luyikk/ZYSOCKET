using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ZYSocket.share;

namespace ZYSocket.RPCX.Service
{
    public delegate void CallBufferSendHanlder(byte[] data);


    public class RPC
    {
        /// <summary>
        /// 接口表
        /// </summary>
        public ConcurrentDictionary<Type, ZYProxy> ZYProxyDiy { get; set; }

        /// <summary>
        /// 返回值表
        /// </summary>
        public ConcurrentDictionary<long, TaskCompletionSource<Result_Have_Return>> ResultDiy { get; set; }

        public ConcurrentDictionary<string, ModuleDef> ModuleDiy { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }


        public event CallBufferSendHanlder CallBufferOutSend;


        private bool IsCallReturn { get; set; }

        public RPC(bool isCallReturn)
        {
            OutTime = 2000;
            IsCallReturn = isCallReturn;
            ZYProxyDiy = new ConcurrentDictionary<Type, ZYProxy>();
            ResultDiy = new ConcurrentDictionary<long, TaskCompletionSource<Result_Have_Return>>();
            ModuleDiy = new ConcurrentDictionary<string, ModuleDef>();
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
                ZYProxy proxy = new ZYProxy(type, IsCallReturn);

                proxy.CallHaveReturn += CallHaveReturn;
                proxy.CallNullReturn += CallNullReturn;

                ZYProxyDiy.AddOrUpdate(type, proxy, (x, y) => proxy);

                return (T)proxy.GetTransparentProxy();
            }
        }



        #region NULL RETURN
        private void CallNullReturn(string Tag, string MethodName, Type[] argTypelist,List<byte[]> arglist)
        {
            CallMethod(Tag, MethodName, argTypelist, arglist);
        }
        
        /// <summary>
        /// 调用NULL返回值的函数，out ref将失效,不等待同步
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="methodName"></param>
        /// <param name="argTypeList"></param>
        /// <param name="arglist"></param>
        public void CallMethod(string Tag,string methodName,Type[] argTypeList, List<byte[]> arglist)
        {
            RPCCallPack callpack = new RPCCallPack()
            {
                Tag = Tag,
                Method = methodName,
                Arguments = arglist,
                NeedReturn = false,
                Id = Make.GetID()
            };

            byte[] data = BufferFormat.FormatFCA(callpack);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);
        }
        #endregion
        
        #region RETURN
        private ProxyReturnValue CallHaveReturn(string Tag, string MethodName, Type[] argTypelist, List<byte[]> arglist, Type returnType)
        {
            ProxyReturnValue tmp = new ProxyReturnValue();

            object[] args;

            tmp.returnVal = CallMethod(Tag, MethodName, argTypelist, arglist, out args, returnType);
                     
            tmp.Args = args;

            return tmp;
        }


        public object CallMethod(string tag, string methodName, Type[] argTypelist, List<byte[]> arglist, out object[] args, Type returnType)
        {
            args = null;

            RPCCallPack call = new RPCCallPack()
            {
                Id = Make.GetID(),
                Tag = tag,
                Method = methodName,
                Arguments = arglist,
                NeedReturn = true
            };


            TaskCompletionSource<Result_Have_Return> var = new TaskCompletionSource<Result_Have_Return>(TaskCreationOptions.AttachedToParent);

            if (!ResultDiy.TryAdd(call.Id, var))
            {
                SpinWait.SpinUntil(() => ResultDiy.TryAdd(call.Id, var));
            }


            byte[] data = BufferFormat.FormatFCA(call);
            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            if (var.Task.Wait(OutTime))
            {
                Result_Have_Return returnx = var.Task.Result;

                if (returnx.Arguments != null && returnx.Arguments.Count > 0 && arglist.Count == returnx.Arguments.Count)
                {
                    args = new object[returnx.Arguments.Count];

                    for (int i = 0; i < argTypelist.Length; i++)
                    {
                        args[i] = Serialization.UnpackSingleObject(argTypelist[i], returnx.Arguments[i]);
                    }

                }

                if (returnx.Return != null)
                {
                    object returnobj = Serialization.UnpackSingleObject(returnType, returnx.Return);
                    return returnobj;
                }
                else
                    return null;
            }
            else
            {
                if (!ResultDiy.TryRemove(call.Id, out var))
                {
                    SpinWait.SpinUntil(() => ResultDiy.TryRemove(call.Id, out var));
                }

                LogAction.Warn(tag + "->" + methodName + " out time,Please set the timeout time.");

                return null;

            }

        }


        /// <summary>
        /// 设置返回值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetReturnValue(Result_Have_Return val)
        {
            if (ResultDiy.ContainsKey(val.Id))
            {
                var x = ResultDiy[val.Id];                       
                x.SetResult(val);
                ResultDiy.TryRemove(val.Id, out x);
                return true;
            }
            
            return false;
        }
        #endregion

        #region MODULE

        public void RegModule(RPCCallObject o)
        {
            ModuleDef tmp = new ModuleDef(o);          
            ModuleDiy.AddOrUpdate(o.GetType().Name, tmp, (a, b) => tmp);

        }

        /// <summary>
        /// 调用CALL
        /// </summary>
        /// <param name="tmp">调用包</param>
        /// <param name="needReturn">是否需要返回</param>
        /// <param name="returnValue">返回值</param>
        /// <returns></returns>
        public bool RunModule(RPCCallPack tmp,bool needReturn, out object returnValue)
        {

            returnValue = null;

            try
            {

                if (ModuleDiy.ContainsKey(tmp.Tag))
                {
                    var module = ModuleDiy[tmp.Tag];

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

                            if (needReturn)
                            {

                                if (method.IsOut)
                                {
                                    for (int i = 0; i < arguments.Length; i++)
                                    {
                                        tmp.Arguments[i] = Serialization.PackSingleObject(method.ArgsType[i], arguments[i]);
                                    }
                                }
                                else
                                    tmp.Arguments = null;

                            }

                            return true;

                        }
                        else
                        {
                            returnValue = method.methodInfo.Invoke(module.Token, null);
                            return true;
                        }
                    }
                    else
                    {

                        LogAction.Warn("Not find " + tmp.Tag + "-> public " + tmp.Method);

                        return false;
                    }
                }
                else
                {
                    LogAction.Warn("Not find " + tmp.Tag);
                }

                return false;
            }
            catch (Exception er)
            {
                LogAction.Err(er.ToString());
                return false;
            }
        }


        #endregion
    }
}
