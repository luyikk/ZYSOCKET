using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MsgPack.Serialization;
using ZYSocket.ClientB;
using ZYSocket.share;
using System.Collections.Concurrent;
using System.Threading;

namespace ZYSocket.RPC.Client
{
   

    public class RPCClient
    {

        static RPCClient()
        {
            SerializationContext.Default.GetSerializer<ZYClientCall>();
        }

        public SocketClient Client { get; set; }

        public ZYNetRingBufferPoolV2 Stream { get; set; }

        public event ClientBinaryInputHandler DataOn;

        public event ClientMessageInputHandler Disconn;
      

        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }

        /// <summary>
        /// 是否连接
        /// </summary>
        protected bool IsConnect { get; set; }

        public ConcurrentDictionary<string, ResReturn> ResRetrunDiy { get; set; }


        public RPCClient()
        {
            OutTime = 8000;

        }

        void Client_MessageInput(string message)
        {
            IsConnect = false;          

            if (Disconn != null)
                Disconn(message);
        }

        void Client_BinaryInput(byte[] data)
        {

            Stream.Write(data);

            byte[] datax;
            while (Stream.Read(out datax))
            {
                BinaryInput(datax);
            }
        }


        public void BinaryInput(byte[] data)
        {
            ReadBytesV2 read = new ReadBytesV2(data);

            int lengt;
            int cmd;

            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 1001001:
                        {
                            ZYClient_Result_Return val;

                            if (read.ReadObject<ZYClient_Result_Return>(out val))
                            {
                                if (ResRetrunDiy.ContainsKey(val.Id))
                                {
                                    ResRetrunDiy[val.Id].ReturnValue = val;
                                    ResRetrunDiy[val.Id].waitHander.Set();
                                }

                            }

                        }
                        break;
                    default:
                        {
                            if (DataOn != null)
                                DataOn(data);
                        }
                        break;
                }


            }

        }



        public bool Connection(string host, int port)
        {
            if (!IsConnect)
            {
                ResRetrunDiy = new ConcurrentDictionary<string, ResReturn>();
                Stream = new ZYNetRingBufferPoolV2(1024 * 1024 * 30);
                Client = new SocketClient();
                Client.BinaryInput += Client_BinaryInput;
                Client.MessageInput += Client_MessageInput;

                if (Client.Connect(host, port))
                {
                    IsConnect = true;
                    Client.StartRead();
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }

        public void SendData(byte[] data)
        {
            if (IsConnect)
            {
                Client.Send(data);
            }
        }


        public Result Call<Mode, Result>(Expression<Func<Mode, Result>> action)
        {           

            MethodCallExpression body = action.Body as MethodCallExpression;

            if (body != null && IsConnect)
            {

                string callModule = body.Object.Type.FullName;

                string method = body.Method.Name;

                List<string> ArgumentsType = new List<string>();

                List<byte[]> Arguments = new List<byte[]>();

                List<Type> ArgType = new List<Type>();

                foreach (var item in body.Arguments)
                {
                    var p = Expression.Call(null, this.GetType().GetMethod("GetObjectArg", new[] { item.Type }), item);
                    object x = Expression.Lambda<Func<object>>(p).Compile()();

                    if (x != null)
                    {
                        ArgType.Add(x.GetType());
                        ArgumentsType.Add(x.GetType().FullName);
                        Arguments.Add(MsgPack.Serialization.SerializationContext.Default.GetSerializer(x.GetType()).PackSingleObject(x));
                    }
                    else 
                    {
                        Type type = item.Type;

                        ArgType.Add(type);
                        ArgumentsType.Add(type.FullName);
                        Arguments.Add(MsgPack.Serialization.SerializationContext.Default.GetSerializer(type).PackSingleObject(null));
                    }
                }

                ZYClientCall call = new ZYClientCall()
                {
                    Id = Guid.NewGuid().ToString(),
                    CallTime = DateTime.Now,
                    CallModule = callModule,
                    Method = method,
                    Arguments = Arguments,
                    IsNeedReturn = true,
                    ArgumentsType = ArgumentsType
                };

                Client.Send(BufferFormatV2.FormatFCA(call));

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

                        Type type = Type.GetType(returnx.ReturnType);

                        if (type == null)
                            type = typeof(Result);

                        object returnobj = MsgPack.Serialization.SerializationContext.Default.GetSerializer(type).UnpackSingleObject(returnx.Return);



                        if (returnx.Arguments != null && returnx.Arguments.Count > 0 && ArgType.Count == returnx.Arguments.Count)
                        {
                            object[] args = new object[returnx.Arguments.Count];

                            for (int i = 0; i < returnx.Arguments.Count; i++)
                            {
                                args[i] = MsgPack.Serialization.SerializationContext.Default.GetSerializer(ArgType[i]).UnpackSingleObject(returnx.Arguments[i]);
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

            }

            return default(Result);
        }

        public void Call<Mode>(Expression<Action<Mode>> action)
        {
          

            MethodCallExpression body = action.Body as MethodCallExpression;

            if (body != null && IsConnect)
            {

                string callModule = body.Object.Type.FullName;

                string method = body.Method.Name;

                List<string> ArgumentsType = new List<string>();

                List<byte[]> Arguments = new List<byte[]>();

                List<Type> ArgType = new List<Type>();


                foreach (var item in body.Arguments)
                {
                    var p = Expression.Call(null, this.GetType().GetMethod("GetObjectArg", new[] { item.Type }), item);
                    object x = Expression.Lambda<Func<object>>(p).Compile()();

                    if (x != null)
                    {
                        ArgType.Add(x.GetType());
                        ArgumentsType.Add(x.GetType().FullName);
                        Arguments.Add(MsgPack.Serialization.SerializationContext.Default.GetSerializer(x.GetType()).PackSingleObject(x));
                    }
                    else
                    {
                        Type type = item.Type;

                        ArgType.Add(type);
                        ArgumentsType.Add(type.FullName);
                        Arguments.Add(MsgPack.Serialization.SerializationContext.Default.GetSerializer(type).PackSingleObject(null));
                    }
                }

                ZYClientCall call = new ZYClientCall()
                {
                    Id = Guid.NewGuid().ToString(),
                    CallTime = DateTime.Now,
                    CallModule = callModule,
                    Method = method,
                    Arguments = Arguments,
                    IsNeedReturn = true,
                    ArgumentsType = ArgumentsType
                };

                Client.Send(BufferFormatV2.FormatFCA(call));

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


                        if (returnx.Arguments != null && returnx.Arguments.Count > 0 && ArgType.Count == returnx.Arguments.Count)
                        {
                            object[] args = new object[returnx.Arguments.Count];

                            for (int i = 0; i < returnx.Arguments.Count; i++)
                            {
                                args[i] = MsgPack.Serialization.SerializationContext.Default.GetSerializer(ArgType[i]).UnpackSingleObject(returnx.Arguments[i]);
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

    public class ResReturn
    {
        public EventWaitHandle waitHander { get; set; }

        public ZYClient_Result_Return ReturnValue { get; set; }

    }
}
