#if!Net2
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZYSocket.MicroThreading
{

    public abstract class ChannelMicroThreadAwaiterBase : ICriticalNotifyCompletion
    {
       

        protected bool isCompleted = false;

        internal MicroThread MicroThread;
        internal Action Continuation;
        public abstract void OnCompleted(Action continuation);
        public abstract void UnsafeOnCompleted(Action continuation);

        public bool IsCompleted
        {
            get { return isCompleted || (MicroThread != null && MicroThread.IsOver); }
            set { isCompleted = value; }
        }

    }



    public class ChannelMicroThreadAwaiter<T> : ChannelMicroThreadAwaiterBase
    {

        private static List<ChannelMicroThreadAwaiter<T>> pool = new List<ChannelMicroThreadAwaiter<T>>();

        internal T Result;

        public static ChannelMicroThreadAwaiter<T> New(MicroThread microThread)
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    var index = pool.Count - 1;
                    var lastItem = pool[index];
                    pool.RemoveAt(index);

                    lastItem.MicroThread = microThread;

                    return lastItem;
                }

                return new ChannelMicroThreadAwaiter<T>(microThread);
            }
        }

        public ChannelMicroThreadAwaiter(MicroThread microThread)
        {
            MicroThread = microThread;
        }

        public ChannelMicroThreadAwaiter<T> GetAwaiter()
        {
            return this;
        }

        public  override void OnCompleted(Action continuation)
        {
            Continuation = continuation;
        }

        public override  void UnsafeOnCompleted(Action continuation)
        {
            Continuation = continuation;
        }

        public T GetResult()
        {
            // Check Task Result (exception, etc...)
            MicroThread.CancellationToken.ThrowIfCancellationRequested();

            var result = Result;

            // After result has been taken, we can reuse this item, so put it in the pool
            // We mitigate pool size, but another approach than hard limit might be interesting
            lock (pool)
            {
                if (pool.Count < 4096)
                {
                    isCompleted = false;
                    MicroThread = null;
                    Continuation = null;
                    Result = default(T);
                }

                pool.Add(this);
            }

            return result;
        }


     

    }
}
#endif