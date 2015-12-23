using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYSocket.Server;
using System.Net.Sockets;

namespace ZYSocket.EnsureSend
{
    public class ZYEnsureSend : IEnsureSend
    {
        static ZYEnsureSend()
        {
            TaskQueue<Socket>.StopExcption.Add(typeof(SocketException));
        }

        protected RingBuffer BufferPool { get; private set; }

        protected TaskQueue<Socket> TaskQueuePool { get; set; }

        public SocketAsyncEventArgs Asyn { get; private set; }

        public ZYEnsureSend(SocketAsyncEventArgs asyn) : this(asyn,1024*64) { }    
        
        public ZYEnsureSend(SocketAsyncEventArgs asyn,int maxSize)
        {
            Asyn = asyn;

           
            BufferPool = new RingBuffer(maxSize);
            TaskQueuePool = new TaskQueue<Socket>();          

        }
        

        public bool EnsureSend(byte[] data)
        {
           
            try
            {
                TaskQueuePool.CheckError();

                if (!BufferPool.Write(data))
                    return false;

                if (TaskQueuePool.ActionQueue.Count > 1 && TaskQueuePool.RunTask.Status == System.Threading.Tasks.TaskStatus.Running)
                    return true;

                TaskQueuePool.Push(new ActionRun<Socket>((sock) =>
                {
                    if (BufferPool.Length == 0)
                        return true;

                    while (BufferPool.Length > 0)
                    {

                        int lengt = BufferPool.Length;

                        byte[] pdata = BufferPool.ReadNoPostion(lengt);

                        int sendlengt = sock.Send(pdata, 0, pdata.Length, SocketFlags.None);

                        BufferPool.SubLength(sendlengt);
                    }

                    if (BufferPool.Length == 0)
                        return true;
                    else
                        return false;

                }, Asyn.AcceptSocket));

                return true;

            }
            catch (Exception er)
            {
                throw er;
            }
           
        }
    }
}
