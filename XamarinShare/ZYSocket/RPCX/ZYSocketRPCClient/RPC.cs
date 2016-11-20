using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;

namespace ZYSocket.RPCX.Client
{
    public delegate void CallBufferSendHanlder(byte[] data);

    public class RPC
    {
        /// <summary>
        /// 接口表
        /// </summary>
        public Dictionary<Type, ZYProxy> ZYProxyDiy { get; set; }

        public Dictionary<long, WaitReturnValue> ReturnValueDiy { get; set; }

        public ModuleDictionary Module { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }


        public event CallBufferSendHanlder CallBufferOutSend;

        public RPC()
        {
            OutTime = 2000;
            ZYProxyDiy = new Dictionary<Type, ZYProxy>();
            ReturnValueDiy = new Dictionary<long, WaitReturnValue>();
            Module = new ModuleDictionary();
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

                proxy.CallHaveReturn += CallHaveReturn;
                proxy.CallNullReturn += CallNullReturn;

                ZYProxyDiy.Add(type, proxy);

                return (T)proxy.GetTransparentProxy();
            }
        }


        #region NULL RETURN
        private void CallNullReturn(string Tag, string MethodName, Type[] argTypelist, List<byte[]> arglist)
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
        public void CallMethod(string Tag, string methodName, Type[] argTypeList, List<byte[]> arglist)
        {
            RPCCallPack callpack = new RPCCallPack()
            {
                Tag = Tag,
                Method = methodName,
                Arguments = arglist,
                NeedReturn = false               
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
                Tag = tag,
                Method = methodName,
                Arguments = arglist,
                NeedReturn = true
            };


            WaitReturnValue var = new WaitReturnValue();

            lock(ReturnValueDiy)
            {
                call.Id = Make.GetID();

               
                ReturnValueDiy.Add(call.Id, var);

            }

            byte[] data = BufferFormat.FormatFCA(call);
            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            if (var.waitHandle.WaitOne(OutTime))
            {
                Result_Have_Return returnx = var.returnvalue;

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
                lock(ReturnValueDiy)
                {
                    ReturnValueDiy.Remove(call.Id);
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
            lock(ReturnValueDiy)
            {
                if (ReturnValueDiy.ContainsKey(val.Id))
                {
                    var x = ReturnValueDiy[val.Id];
                    x.returnvalue = val;
                    ReturnValueDiy.Remove(val.Id);
                    x.waitHandle.Set();
                    return true;
                }

                return false;
            }
        }
        #endregion


        #region MODULE

        public void RegModule(object o)
        {
            Module.Install(o);
        }

        /// <summary>
        /// 调用CALL
        /// </summary>
        /// <param name="tmp">调用包</param>
        /// <param name="needReturn">是否需要返回</param>
        /// <param name="returnValue">返回值</param>
        /// <returns></returns>
        public bool RunModule(RPCCallPack tmp, bool needReturn, out object returnValue)
        {

            returnValue = null;

            try
            {
                var method = Module.GetMethod(tmp.Tag, tmp.Method);

                if (method != null)
                {                    
                    if (tmp.Arguments != null)
                    {

                        object[] arguments = new object[method.ArgsType.Length];


                        for (int i = 0; i < tmp.Arguments.Count; i++)
                        {
                            arguments[i] = Serialization.UnpackSingleObject(method.ArgsType[i], tmp.Arguments[i]);
                        }

                        returnValue = method.methodInfo.Invoke(method.Token, arguments);

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
                        returnValue = method.methodInfo.Invoke(method.Token, null);
                        return true;
                    }

                }
                else
                {
                    LogAction.Warn("Not find " + tmp.Tag + "-> public " + tmp.Method);
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
