using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ZYSocket.share;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;

namespace ZYSocket.RPC
{


    public delegate void CallBufferOutHanlder(byte[] data);

    public class RPC
    {

        public FormatValueType FormatValue { get; set; }

        public ConcurrentDictionary<string, ResReturn> ResRetrunDiy { get; set; }

        public ConcurrentDictionary<string, Action<AsynReturn>> AsynRetrunDiy { get; set; }

        public ConcurrentDictionary<string, object> ModuleDiy { get; set; }


        public event CallBufferOutHanlder CallBufferOutSend;
        

        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }


        public RPC()
        {
            FormatValue = new FormatValueType();
            OutTime = 800;
            ResRetrunDiy = new ConcurrentDictionary<string, ResReturn>();
            ModuleDiy = new ConcurrentDictionary<string, object>();
            AsynRetrunDiy = new ConcurrentDictionary<string, Action<AsynReturn>>();
        }

        /// <summary>
        /// 设置返回值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetReturnValue(ZYClient_Result_Return val)
        {
            if (ResRetrunDiy.ContainsKey(val.Id))
            {
                ResRetrunDiy[val.Id].ReturnValue = val;
                ResRetrunDiy[val.Id].waitHander.Set();
                return true;
            }
            else if (AsynRetrunDiy.ContainsKey(val.Id))
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

            return false;
        }

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
                Id = Guid.NewGuid().ToString(),
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
                Id = Guid.NewGuid().ToString(),
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
                Id = Guid.NewGuid().ToString(),
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
        /// 同步调用 返回
        /// </summary>
        /// <typeparam name="Mode"></typeparam>
        /// <typeparam name="Result"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public Result Call<Mode, Result>(Expression<Func<Mode, Result>> action)
        {
            MethodCallExpression body = action.Body as MethodCallExpression;

            List<RPCArgument> argumentlist = new List<RPCArgument>();

            var parameters= body.Method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {              
                dynamic p = Expression.Call(FormatValue.GetMethodInfo(body.Arguments[i].Type), body.Arguments[i]);
                dynamic x = Expression.Lambda<Func<dynamic>>(p).Compile()();

                RPCArgument tmp = new RPCArgument();
                tmp.type = body.Arguments[i].Type;
                tmp.RefType=parameters[i].ParameterType;
                tmp.Value = MsgPackSerialization.GetMsgPack(body.Arguments[i].Type).PackSingleObject(x);
                argumentlist.Add(tmp);
            }

         

            RPCCallPack call = new RPCCallPack()
            {
                Id = Guid.NewGuid().ToString(),
                CallTime = DateTime.Now,
                CallModule = body.Object.Type.Name,
                Method = body.Method.Name,
                Arguments = argumentlist,
                IsNeedReturn = true,               
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            using (EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                ResReturn var = new ResReturn()
                {
                    waitHander = wait,
                    ReturnValue = null
                };

                ResRetrunDiy.AddOrUpdate(call.Id, var, (a, b) => var);

                if (wait.WaitOne(OutTime))
                {

                    ResRetrunDiy.TryRemove(call.Id, out var);

                    ZYClient_Result_Return returnx = var.ReturnValue;

                  

                    Type type = returnx.ReturnType;

                    if (type == null)
                        type = typeof(Result);


                    object returnobj = MsgPackSerialization.GetMsgPack(type).UnpackSingleObject(returnx.Return);
                    

                    if (returnx.Arguments != null && returnx.Arguments.Count > 0 && argumentlist.Count == returnx.Arguments.Count)
                    {
                        object[] args = new object[returnx.Arguments.Count];

                        for (int i = 0; i < returnx.Arguments.Count; i++)
                        {
                            args[i] = MsgPackSerialization.GetMsgPack(returnx.Arguments[i].type).UnpackSingleObject(returnx.Arguments[i].Value);
                        }


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

                 

                    return (Result)returnobj;

                }

                ResRetrunDiy.TryRemove(call.Id, out var);

            }


            return default(Result);
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


            RPCCallPack call = new RPCCallPack()
            {
                Id = Guid.NewGuid().ToString(),
                CallTime = DateTime.Now,
                CallModule = body.Object.Type.Name,
                Method = body.Method.Name,
                Arguments = argumentlist,
                IsNeedReturn = true,
            };

            byte[] data = BufferFormat.FormatFCA(call);

            if (CallBufferOutSend != null)
                CallBufferOutSend(data);

            using (EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.AutoReset))
            {
                ResReturn var = new ResReturn()
                {
                    waitHander = wait,
                    ReturnValue = null
                };

                ResRetrunDiy.AddOrUpdate(call.Id, var, (a, b) => var);

                if (wait.WaitOne(OutTime))
                {

                    ZYClient_Result_Return returnx = ResRetrunDiy[call.Id].ReturnValue;
                    
                    if (returnx.Arguments != null && returnx.Arguments.Count > 0 && argumentlist.Count == returnx.Arguments.Count)
                    {
                        object[] args = new object[returnx.Arguments.Count];

                        for (int i = 0; i < returnx.Arguments.Count; i++)
                        {
                            args[i] = MsgPackSerialization.GetMsgPack(returnx.Arguments[i].type).UnpackSingleObject(returnx.Arguments[i].Value);
                        }


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

                ResRetrunDiy.TryRemove(call.Id, out var);

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
                    return false;
                }
            }


            return false;
        }

        public class ResReturn
        {
            public EventWaitHandle waitHander { get; set; }

            public ZYClient_Result_Return ReturnValue { get; set; }

        }

   
     
    }
}
