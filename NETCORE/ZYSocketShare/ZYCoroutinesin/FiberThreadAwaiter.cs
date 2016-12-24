#if!Net2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZYSocket.ZYCoroutinesin
{


    public abstract class FiberThreadAwaiterBase : ICriticalNotifyCompletion
    {
        public abstract void OnCompleted(Action continuation);


        public abstract void UnsafeOnCompleted(Action continuation);

        internal Fiber fiber;
        internal Action Continuation;
       

        protected bool isCompleted = false;

        public bool IsCompleted
        {
            get { return isCompleted || (fiber != null && fiber.IsOver); }
            set { isCompleted = value; }
        }

    }


    public class FiberThreadAwaiter<T>: FiberThreadAwaiterBase
    {
       
        internal T Result;

        public static FiberThreadAwaiter<T> New(Fiber _fiber)
        {
            return new FiberThreadAwaiter<T>(_fiber);

        }

        public FiberThreadAwaiter(Fiber GhostThread)
        {
            fiber = GhostThread;
            IsCompleted = false;
        }

        public FiberThreadAwaiter<T> GetAwaiter()
        {
            return this;
        }

        public override void OnCompleted(Action continuation)
        {
            Continuation = continuation;
        }

        public override void UnsafeOnCompleted(Action continuation)
        {
            Continuation = continuation;
        }


        public T GetResult()
        {
            
            fiber.CancellationToken.ThrowIfCancellationRequested();

            var result = Result;           

            return result;
        }

      
    }
}
#endif