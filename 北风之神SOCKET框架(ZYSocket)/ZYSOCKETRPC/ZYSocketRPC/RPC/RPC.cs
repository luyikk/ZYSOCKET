using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ZYSocket.RPC
{


    public delegate void CallBufferOutHanlder(byte[] data);

    public delegate void ErrMsgOutHanlder(string msg);

    public class RPC
    {
               

        public FormatValueType FormatValue { get; set; }


        public ConcurrentDictionary<Type, ZYProxy> ZYProxyDiy { get; set; }


        public ConcurrentDictionary<long, WaitReturnValue> ReturnValueDiy { get; set; }

        public ConcurrentDictionary<long, Action<AsynReturn>> AsynRetrunDiy { get; set; }

        public ConcurrentDictionary<string, object> ModuleDiy { get; set; }


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
           
            ModuleDiy = new ConcurrentDictionary<string, object>();
            AsynRetrunDiy = new ConcurrentDictionary<long, Action<AsynReturn>>();
            ReturnValueDiy = new ConcurrentDictionary<long, WaitReturnValue>();
            ZYProxyDiy = new ConcurrentDictionary<Type, ZYProxy>();
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
                AsynRetrunDiy.TryRemove(val.Id, out callback);

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    callback(asynRet);

                }).ContinueWith(p =>
                    {
                        try
                        {
                            p.Wait();
                        }
                        catch (Exception er)
                        {
                            if (ErrMsgOut != null)
                                ErrMsgOut(er.ToString());
                        }
                    });
            }
            else if (ReturnValueDiy.ContainsKey(val.Id))
            {                
                WaitReturnValue x;
                ReturnValueDiy.TryRemove(val.Id, out x);
                x.returnvalue = val;
                x.waitHandle.Set();
            }
         
          

            return false;
        }

        #region Expression Tree

        /// <summary>
        /// 异步调用，返回值后调用 Callback
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <typeparam name="Result"></typeparam>
        /// <param name="action"></param>
        /// <param name="Callback"></param>
        public void CallAsyn<Mode, Result>(Expression<Func<Mode, Result>> action,Action<AsynReturn> Callback)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters = body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType = parameters[i].ParameterType;
                tmp.Value = Serialization.PackSingleObject(body.Arguments[i].Type,x);
                argumentlist.Add(tmp);
            }



            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = body.Method.Name,
                Arguments = argumentlist,
                IsNeedReturn = true,
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);


            AsynRetrunDiy.AddOrUpdate(call.Id, Callback, (a, b) => Callback);

        }

        /// <summary>
        /// 调用不等待返回
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <param name="action"></param>
        public void CallAsyn<Mode>(Expression<Action<Mode>> action)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters = body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType = parameters[i].ParameterType;
                tmp.Value = Serialization.PackSingleObject(body.Arguments[i].Type,x);
                argumentlist.Add(tmp);
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = body.Method.Name,
                Arguments = argumentlist,
                IsNeedReturn = true,
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);          

        }

        /// <summary>
        /// 调用等待返回
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <param name="action"></param>
        /// <param name="Callback"></param>
        public void CallAsyn<Mode>(Expression<Action<Mode>> action, Action<AsynReturn> Callback)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters = body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType = parameters[i].ParameterType;
                tmp.Value = Serialization.PackSingleObject(body.Arguments[i].Type,x);
                argumentlist.Add(tmp);
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = body.Method.Name,
                Arguments = argumentlist,
                IsNeedReturn = true,
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            AsynRetrunDiy.AddOrUpdate(call.Id, Callback, (a, b) => Callback);

        }



        /// <summary>
        /// 同步调用 返回 适用于返回void
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <param name="action"></param>
        public void Call<Mode>(Expression<Action<Mode>> action)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters = body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType = parameters[i].ParameterType;
                tmp.Value = Serialization.PackSingleObject(body.Arguments[i].Type,x);
                argumentlist.Add(tmp);
            }

            object[] args;

            CallMethod(body.Object.Type.Name, body.Method.Name, argumentlist, out args);

            if (args != null)
            {
                if (args.Length == body.Arguments.Count)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (body.Arguments[i].NodeType == ExpressionType.MemberAccess)
                        {
                            var set = Expression.Assign(body.Arguments[i], Expression.Constant(args[i]));

                            Expression.Lambda<Action>(set).Compile()();

                        }
                    }
                }
            }
        }


     


        /// <summary>
        /// 同步调用返回
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <typeparam name="Result"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public Result Call<Mode, Result>(Expression<Func<Mode, Result>> action)
        {


            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters = body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType = parameters[i].ParameterType;
                tmp.Value = Serialization.PackSingleObject(body.Arguments[i].Type,x);
                argumentlist.Add(tmp);


            }

            object[] args;

            Result res = CallMethod<Result>(body.Object.Type.Name, body.Method.Name, argumentlist, out args);

            if (args != null)
            {
                if (args.Length == body.Arguments.Count)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (body.Arguments[i].NodeType == ExpressionType.MemberAccess)
                        {
                            var set = Expression.Assign(body.Arguments[i], Expression.Constant(args[i]));

                            Expression.Lambda<Action>(set).Compile()();

                        }
                    }
                }
            }

            return res;
        }

        


        #endregion



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

                ZYProxyDiy.AddOrUpdate(type,proxy,(x,y)=>proxy);

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
                CallTime = MakeID.GetTick(),
                CallModule = module,
                Method = MethodName,
                Arguments = arglist,
                IsNeedReturn = true,
            };

            WaitReturnValue var = new WaitReturnValue();

            using (var.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {

                ReturnValueDiy.AddOrUpdate(call.Id, var, (a, b) => var);

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
                    ReturnValueDiy.TryRemove(call.Id, out var);

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
                CallTime = MakeID.GetTick(),
                CallModule = module,
                Method = MethodName,
                Arguments = arglist,
                IsNeedReturn = true,
            };



            WaitReturnValue var = new WaitReturnValue();
            using (var.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
            {


                ReturnValueDiy.TryAdd(call.Id, var);

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

                    ReturnValueDiy.TryRemove(call.Id, out var);

                    throw new TimeoutException("out time,Please set the timeout time.");
                  
                }
            }

        }


        public void RegModule(object o)
        {
            Type type = o.GetType();

            ModuleDiy.AddOrUpdate(type.Name, o, (a, b) => o);

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
