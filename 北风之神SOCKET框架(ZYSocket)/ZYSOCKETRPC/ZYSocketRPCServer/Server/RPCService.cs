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
        public bool CallModule(byte[] data, RPCUserInfo e,out ReadBytes read,out int cmd)
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

                                System.Threading.Tasks.Task.Factory.StartNew(() =>
                                 {
                                     object returnValue;

                                     CallContext.SetData("Current", e);

                                     if (e.RPC_Call.RunModule(tmp, out returnValue))
                                     {
                                         if (tmp.IsNeedReturn)
                                         {
                                             ZYClient_Result_Return var = new ZYClient_Result_Return()
                                             {
                                                 Id = tmp.Id,
                                                 CallTime = tmp.CallTime,
                                                 Arguments = tmp.Arguments
                                             };

                                             if (returnValue != null)
                                             {
                                                 var.Return = Serialization.PackSingleObject(returnValue.GetType(), returnValue);
                                                 var.ReturnType = returnValue.GetType();
                                             }

                                             e.BeginSendData(BufferFormat.FormatFCA(var));
                                             //e.EnsureSend(BufferFormat.FormatFCA(var));
                                         }

                                     }

                                 }, CancellationToken.None, TaskCreationOptions.None, e.QueueScheduler).ContinueWith(p =>
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
                


        public void Disconnect(RPCUserInfo e)
        {
            foreach (var item in e.RPC_Call.ModuleDiy.Values)
            {
                Type type = item.GetType();

                if (type.BaseType == typeof(RPCObject))
                {
                    type.GetMethod("ClientDisconnect").Invoke(item, new[] { e });

                }

            }
        }

    }
}
