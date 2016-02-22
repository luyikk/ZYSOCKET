using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using ZYSocket.share;

namespace ZYSocket.RPCX.Service
{
    public class RPCService
    {
        /// <summary>
        /// 调用模块
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true 属于次模块,false 不属于此模块数据</returns>
        public bool CallModule(byte[] data, RPCUserInfo e, bool isTaskQueue, out ReadBytes read, out int cmd)
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
                                   Task.Factory.StartNew((pack) => Call(pack as RPCCallPack, e)
                                    , tmp, CancellationToken.None, TaskCreationOptions.None, e.Scheduler).ContinueWith(p =>
                                    {                                    
                                        try
                                        {
                                            p.Wait();
                                        }
                                        catch
                                        {
                                         
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
                            Result_Have_Return val;

                            if (read.ReadObject<Result_Have_Return>(out val))
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
                               
                if (e.RPC_Call.RunModule(rpcPack, rpcPack.NeedReturn, out returnValue))
                {
                    if (rpcPack.NeedReturn)
                    {
                        Result_Have_Return var = new Result_Have_Return()
                        {
                            Id = rpcPack.Id,
                            Arguments = rpcPack.Arguments,

                        };

                        if (returnValue != null)
                        {
                            var.Return = Serialization.PackSingleObject(returnValue.GetType(), returnValue);
                        }

                        e.SendData(BufferFormat.FormatFCA(var));
                    }
                }
            }
            catch (Exception er)
            {

                if (e.Asyn.RemoteEndPoint != null)
                    LogAction.Err(e.Asyn.RemoteEndPoint.ToString() + "::" + rpcPack.Tag + "->" + rpcPack.Method + "\r\n" + er.ToString());
                else
                    LogAction.Err(rpcPack.Tag + "->" + rpcPack.Method + "\r\n" + er.ToString());

            }
        }

        public void Disconnect(RPCUserInfo e)
        {
            foreach (var item in e.RPC_Call.ModuleDiy.Values)
            {
                item.Token.ClientDisconnect(e);
            }
        }
    }
}
