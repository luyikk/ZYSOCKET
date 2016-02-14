using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Sockets;
using ZYSocket.share;
using ZYSocket.RPC;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks.Schedulers;
using System.Threading.Tasks;
using System.Threading;

namespace ZYSocket.RPC.Server
{
    public class RPCService
    {
      

        public event MsgOutHandler MsgOut;
        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true 属于次模块,false 不属于此模块数据</returns>
        public bool CallModule(byte[] data, RPCUserInfo e,bool isTaskQueue,out ReadBytes read,out int cmd)
        {
           

            cmd = -1;

            read = new ReadBytes(data);

            int lengt;          

            if (read.ReadInt32(out lengt) && read.Length == lengt && read.ReadInt32(out cmd))
            {
                switch (cmd)
                {
                    case 1001000:
                        {

                            RPCCallPack tmp;

                            if (read.ReadObject<RPCCallPack>(out tmp))
                            {
                                if (isTaskQueue)
                                {
                                    System.Threading.Tasks.Task.Factory.StartNew((pack) => Call(pack as RPCCallPack, e)
                                    , tmp, CancellationToken.None, TaskCreationOptions.None, e.Scheduler).ContinueWith(p =>
                                          {
                                              try
                                              {
                                                  p.Wait();
                                              }
                                              catch (Exception er)
                                              {
                                                  if (MsgOut != null)
                                                      MsgOut(er.ToString());
                                              }

                                          });


                                    return true;
                                }
                                else
                                {
                                    Call(tmp, e);
                                }
                            }

                        }
                        break;
                    case 1001001:
                        {
                            ZYClient_Result_Return val;

                            if (read.ReadObject<ZYClient_Result_Return>(out val))
                            {
                                e.RPC_Call.SetReturnValue(val);

                                return true;
                            }
                        }
                        break;

                }
            }

            return false;

        }



        private void Call(RPCCallPack rpcPack, RPCUserInfo e)
        {

            try
            {
                object returnValue;

                CallContext.SetData("Current", e);
                string msg;
                if (e.RPC_Call.RunModule(rpcPack, out msg, out returnValue))
                {

                    ZYClient_Result_Return var = new ZYClient_Result_Return()
                    {
                        Id = rpcPack.Id,
                        CallTime = rpcPack.CallTime,
                        Arguments = rpcPack.Arguments,
                        IsSuccess = true                       
                    };

                    if (returnValue != null)
                    {
                        var.Return = Serialization.PackSingleObject(returnValue.GetType(), returnValue);
                    }

                    e.BeginSendData(BufferFormat.FormatFCA(var));
                }
                else
                {

                    ZYClient_Result_Return var = new ZYClient_Result_Return()
                    {
                        Id = rpcPack.Id,
                        CallTime = rpcPack.CallTime,
                        IsSuccess = false,
                        Message = msg
                    };



                    e.BeginSendData(BufferFormat.FormatFCA(var));
                }
            }
            catch (Exception er)
            {               

                ZYClient_Result_Return var = new ZYClient_Result_Return()
                {
                    Id = rpcPack.Id,
                    CallTime = rpcPack.CallTime,
                    IsSuccess = false,
                    Message = er.InnerException!=null?er.InnerException.Message:er.Message
                };

                e.BeginSendData(BufferFormat.FormatFCA(var));

                if (MsgOut != null)
                    MsgOut(er.ToString());
            }
        }

        public void Disconnect(RPCUserInfo e)
        {
            foreach (var item in e.RPC_Call.ModuleDiy.Values)
            {
                Type type = item.Token.GetType();

                if (type.BaseType == typeof(RPCObject))
                {
                    type.GetMethod("ClientDisconnect").Invoke(item.Token, new[] { e });

                }

            }
        }

    }
}
