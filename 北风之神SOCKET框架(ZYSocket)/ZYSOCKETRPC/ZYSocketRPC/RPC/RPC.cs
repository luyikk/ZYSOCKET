using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.share;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq.Expressions;
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

        public ConcurrentDictionary<long, AsynRetrunModule> AsynRetrunDiy { get; set; }

        public ConcurrentDictionary<string, ModuleDef> ModuleDiy { get; set; }


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

            ModuleDiy = new ConcurrentDictionary<string, ModuleDef>();
            AsynRetrunDiy = new ConcurrentDictionary<long, AsynRetrunModule>();
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
                    ReturnValue = val
                };

                var callback = AsynRetrunDiy[val.Id];

                asynRet.Format(callback.ReturnType, callback.ArgsType);


                AsynRetrunDiy.TryRemove(val.Id, out callback);

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    callback.Call(asynRet);

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
                return true;

            }
            else if (ReturnValueDiy.ContainsKey(val.Id))
            {
                var x = ReturnValueDiy[val.Id];
                //x.returnvalue = val;
                //x.IsReturn = true;          
                x.SetResult(val);
                ReturnValueDiy.TryRemove(val.Id, out x);
               

                return true;
            }
         
          

            return false;
        }

        
        #region Expression Tree

        /*


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

            var parameters = body.Method.GetParameters();
            List<byte[]> argumentlist = new List<byte[]>(parameters.Length);      
            List<Type> argTypelist = new List<Type>(parameters.Length);     
            Type[] argRefTypelist = new Type[parameters.Length];     


            for (int i = 0; i < parameters.Length; i++)
            {
                var p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                object x = Expression.Lambda<Func<object>>(p).Compile()();
                argTypelist.Add(body.Arguments[i].Type);
                argumentlist.Add(Serialization.PackSingleObject(body.Arguments[i].Type, x));
                argRefTypelist[i]=parameters[i].ParameterType;
            }

         

            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = MakeID.MakeMethodName(body.Method.Name, argRefTypelist),
                Arguments = argumentlist            
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);


            AsynRetrunModule tmp = new AsynRetrunModule()
            {
                ReturnType=null,
                ArgsType = argTypelist,
                Call = Callback
            };

            AsynRetrunDiy.AddOrUpdate(call.Id, tmp, (a, b) => tmp);

        }

        /// <summary>
        /// 调用不等待返回
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <param name="action"></param>
        public void CallAsyn<Mode>(Expression<Action<Mode>> action)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            var parameters = body.Method.GetParameters();
            List<byte[]> argumentlist = new List<byte[]>(parameters.Length);
            Type[] argRefTypelist = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                object x = Expression.Lambda<Func<object>>(p).Compile()();
                argumentlist.Add(Serialization.PackSingleObject(body.Arguments[i].Type, x));
                argRefTypelist[i] = parameters[i].ParameterType;
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = MakeID.MakeMethodName(body.Method.Name, argRefTypelist),
                Arguments = argumentlist
             
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

            var parameters = body.Method.GetParameters();
            List<byte[]> argumentlist = new List<byte[]>(parameters.Length);
            List<Type> argTypelist = new List<Type>(parameters.Length);
            Type[] argRefTypelist = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                object x = Expression.Lambda<Func<object>>(p).Compile()();
                argTypelist.Add(body.Arguments[i].Type);
                argumentlist.Add(Serialization.PackSingleObject(body.Arguments[i].Type, x));
                argRefTypelist[i] = parameters[i].ParameterType;
            }


            RPCCallPack call = new RPCCallPack()
            {
                Id = MakeID.GetID(),
                CallTime = MakeID.GetTick(),
                CallModule = body.Object.Type.Name,
                Method = MakeID.MakeMethodName(body.Method.Name, argRefTypelist),
                Arguments = argumentlist             
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            AsynRetrunModule tmp = new AsynRetrunModule()
            {
                ReturnType = body.Method.ReturnType,
                ArgsType = argTypelist,
                Call = Callback
            };

            AsynRetrunDiy.AddOrUpdate(call.Id, tmp, (a, b) => tmp);

        }



        /// <summary>
        /// 同步调用 返回 适用于返回void
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <param name="action"></param>
        public void Call<Mode>(Expression<Action<Mode>> action)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            var parameters = body.Method.GetParameters();
            List<byte[]> argumentlist = new List<byte[]>(parameters.Length);
            List<Type> argTypelist = new List<Type>(parameters.Length);
            Type[] argRefTypelist = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
               
                var p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                object x = Expression.Lambda<Func<object>>(p).Compile()();

                argTypelist.Add(body.Arguments[i].Type);
                argumentlist.Add(Serialization.PackSingleObject(body.Arguments[i].Type, x));
                argRefTypelist[i] = parameters[i].ParameterType;
            }



            object[] args;

            CallMethod(body.Object.Type.Name, MakeID.MakeMethodName(body.Method.Name, argRefTypelist), argTypelist, argumentlist, out args);

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
            var parameters = body.Method.GetParameters();


            List<byte[]> argumentlist = new List<byte[]>(parameters.Length);
            List<Type> argTypelist = new List<Type>(parameters.Length);
            Type[] argRefTypelist = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {

                var p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                object x = Expression.Lambda<Func<object>>(p).Compile()();

                argTypelist.Add(body.Arguments[i].Type);
                argumentlist.Add(Serialization.PackSingleObject(body.Arguments[i].Type, x));
                argRefTypelist[i] = parameters[i].ParameterType;
            }



            object[] args;

            Result res = CallMethod<Result>(body.Object.Type.Name, MakeID.MakeMethodName(body.Method.Name, argRefTypelist), argTypelist, argumentlist, out args);

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

    */


        #endregion


        #region proxy

     

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

        #endregion


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

            TaskCompletionSource<ZYClient_Result_Return> var = new TaskCompletionSource<ZYClient_Result_Return>(TaskCreationOptions.AttachedToParent);


            if (!ReturnValueDiy.TryAdd(call.Id, var))
            {
                SpinWait.SpinUntil(() => ReturnValueDiy.TryAdd(call.Id, var));
            }
           

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);
            

            if (var.Task.Wait(OutTime))
            {
                ZYClient_Result_Return returnx = var.Task.Result;

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
                ReturnValueDiy.TryRemove(call.Id, out var);

                throw new TimeoutException("out time,Please set the timeout time.");
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


            TaskCompletionSource<ZYClient_Result_Return> var = new TaskCompletionSource<ZYClient_Result_Return>(TaskCreationOptions.AttachedToParent);

            if (!ReturnValueDiy.TryAdd(call.Id, var))
            {
                SpinWait.SpinUntil(() => ReturnValueDiy.TryAdd(call.Id, var));
            }

            byte[] data = BufferFormat.FormatFCA(call);
             if (CallBufferOutSend != null)
                CallBufferOutSend(data);            

            if (var.Task.Wait(OutTime))
            {

                ZYClient_Result_Return returnx = var.Task.Result;

                if (returnx.IsSuccess)
                {

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
                    throw new TargetException(returnx.Message);

            }
            else
            {
                ReturnValueDiy.TryRemove(call.Id, out var);

                throw new TimeoutException("out time,Please set the timeout time.");

            }


        }


        public void RegModule(object o)
        {
            ModuleDef tmp = new ModuleDef();
            tmp.Init(o);
            ModuleDiy.AddOrUpdate(o.GetType().Name, tmp, (a, b) => tmp);

        }


        public bool RunModule(RPCCallPack tmp,out string Message, out object returnValue)
        {
           
            returnValue = null;
            Message = "finish";
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
                    Message = "Not find " + tmp.CallModule + "-> public " + tmp.Method;

                    if (ErrMsgOut != null)
                        ErrMsgOut(Message);
                

                    return false;
                }
            }
            else
            {
                Message = "Not find " + tmp.CallModule;

                if (ErrMsgOut != null)
                    ErrMsgOut(Message);
            }


            return false;
        }

     
    }

    //public class WaitReturnValue
    //{
    //    public ZYClient_Result_Return returnvalue { get; set; }

    //    //  public EventWaitHandle waitHandle { get; set; }

    //    public bool IsReturn { get; set; }

    //}
}
