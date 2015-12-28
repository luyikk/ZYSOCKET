using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ZYSocket.share;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ZYSocket.RPC
{


    public delegate void CallBufferOutHanlder(byte[] data);

    public delegate void ErrMsgOutHanlder(string msg);

    public class RPC
    {
               

        public FormatValueType FormatValue { get; set; }


        public ConcurrentDictionary<Type, ZYProxy> ZYProxyDiy { get; set; }


        public ConcurrentDictionary<long, TaskCompletionSource<ZYClient_Result_Return>> ReturnValueDiy { get; set; }

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
            ReturnValueDiy = new ConcurrentDictionary<long, TaskCompletionSource<ZYClient_Result_Return>>();
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
                });
            }
            else if (ReturnValueDiy.ContainsKey(val.Id))
            {
                ReturnValueDiy[val.Id].SetResult(val);
                TaskCompletionSource<ZYClient_Result_Return> x;
                ReturnValueDiy.TryRemove(val.Id, out x);
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
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
                argumentlist.Add(tmp);
            }



            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = DateTime.Now,
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
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
                argumentlist.Add(tmp);
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = DateTime.Now,
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
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
                argumentlist.Add(tmp);
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = DateTime.Now,
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
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
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
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
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
                CallTime = DateTime.Now,
                CallModule = module,
                Method = MethodName,
                Arguments = arglist,
                IsNeedReturn = true,
            };

            TaskCompletionSource<ZYClient_Result_Return> var = new TaskCompletionSource<ZYClient_Result_Return>();



            ReturnValueDiy.AddOrUpdate(call.Id, var, (a, b) => var);

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);



            if (var.Task.Wait(OutTime))
            {
                var.Task.Dispose();
                ZYClient_Result_Return returnx = var.Task.Result;

                if (returnx.Arguments != null && returnx.Arguments.Count > 0 && arglist.Count == returnx.Arguments.Count)
                {
                    args = new object[returnx.Arguments.Count];

                    for (int i = 0; i < returnx.Arguments.Count; i++)
                    {
                        args[i] = MsgPackSerialization.GetMsgPack(returnx.Arguments[i].type).UnpackSingleObject(returnx.Arguments[i].Value);
                    }

                }

            }

            ReturnValueDiy.TryRemove(call.Id, out var);

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



            TaskCompletionSource<ZYClient_Result_Return> var = new TaskCompletionSource<ZYClient_Result_Return>();


            ReturnValueDiy.TryAdd(call.Id, var);

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);




            if (var.Task.Wait(OutTime))
            {
                var.Task.Dispose();
                ZYClient_Result_Return returnx = var.Task.Result;

                Type type = returnx.ReturnType;


                if (returnx.Arguments != null && returnx.Arguments.Count > 0 && arglist.Count == returnx.Arguments.Count)
                {
                    args = new object[returnx.Arguments.Count];

                    for (int i = 0; i < returnx.Arguments.Count; i++)
                    {
                        args[i] = MsgPackSerialization.GetMsgPack(returnx.Arguments[i].type).UnpackSingleObject(returnx.Arguments[i].Value);
                    }

                }


                if (type != null)
                {
                    object returnobj = MsgPackSerialization.GetMsgPack(type).UnpackSingleObject(returnx.Return);

                    return (Result)returnobj;
                }
                else
                    return default(Result);

            }
            else
            {


                ReturnValueDiy.TryRemove(call.Id, out var);

                return default(Result);
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
                    arguments[i] = MsgPackSerialization.GetMsgPack(tmp.Arguments[i].type).UnpackSingleObject(tmp.Arguments[i].Value);
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
                        tmp.Arguments[i].Value = MsgPackSerialization.GetMsgPack(arguments[i].GetType()).PackSingleObject(arguments[i]);
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
}
