#if!Net2
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYSocket.ZYCoroutinesin
{
    public class Fiber
    {
        ConcurrentDictionary<Type, ConcurrentQueue<FiberThreadAwaiterBase>> receivers = new ConcurrentDictionary<Type, ConcurrentQueue<FiberThreadAwaiterBase>>();
        ConcurrentDictionary<Type, ConcurrentQueue<FiberThreadAwaiterBase>> senders = new ConcurrentDictionary<Type, ConcurrentQueue<FiberThreadAwaiterBase>>();

        private Func<Task> Action { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        
        private SynchronizationContext previousSyncContext { get; set; }

        private FiberSynchronizationContext _SynchronizationContext { get; set; }

        public bool IsOver { get; private set; }


        public bool IsError { get; private set; }

        public Exception  Error { get; private set; }

        public Fiber()
        {
            _SynchronizationContext = new FiberSynchronizationContext(this);
            cancellationTokenSource = new CancellationTokenSource();
        }


        public static Fiber Current => (SynchronizationContext.Current as FiberSynchronizationContext)?.fiber;

        public void SetAction(Func<Task> action)
        {
            Action = action;
        }

        public void Start()
        {
            IsOver = false;

            Action wrappedGhostThreadFunction = async () =>
            {
                try
                {
                    await Action();


                }
                catch (Exception er)
                {
                    
                    IsError = true;
                    Error = er;
                }
                finally
                {
                    IsOver = true;
                }
            };

            var previousSyncContext = SynchronizationContext.Current;

            SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);          

            wrappedGhostThreadFunction();

            SynchronizationContext.SetSynchronizationContext(previousSyncContext);
        }

        public void Close()
        {
            cancellationTokenSource.Cancel();
        }


        public FiberThreadAwaiter<T> Set<T>(T data)
        {

            Type key = typeof(T);

            if (!receivers.ContainsKey(key) || receivers[key].Count == 0)
            {
                // Nobody receiving, let's wait until something comes up
                var GhostThread = Fiber.Current;

                if (GhostThread == null)
                    GhostThread = this;


                var waitingGhostThread = FiberThreadAwaiter<T>.New(GhostThread);
                waitingGhostThread.Result = data;

                if (senders.ContainsKey(key))
                {
                    senders[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
                else
                {
                    ConcurrentQueue<FiberThreadAwaiterBase> table = new ConcurrentQueue<FiberThreadAwaiterBase>();
                    senders.AddOrUpdate(key, table, (a, b) => table);

                    senders[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
            }



            FiberThreadAwaiterBase tmp;

            if (receivers[key].TryDequeue(out tmp))
            {
                var receiver = tmp as FiberThreadAwaiter<T>;

                receiver.Result = data;
                receiver.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                if(receiver.Continuation!=null)
                    receiver.Continuation();
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);

                return receiver;
            }
            else
                return null;

        }


        public FiberThreadAwaiter<T> Get<T>()
        {
            Type key = typeof(T);

            if (!senders.ContainsKey(key) || senders[key].Count == 0)
            {
                var GhostThread = Fiber.Current;



                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = FiberThreadAwaiter<T>.New(GhostThread);

                if (receivers.ContainsKey(key))
                {
                    receivers[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
                else
                {
                    ConcurrentQueue<FiberThreadAwaiterBase> table = new ConcurrentQueue<FiberThreadAwaiterBase>();
                    receivers.AddOrUpdate(key, table, (a, b) => table);
                    receivers[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
            }

            FiberThreadAwaiterBase sender;

            if (senders[key].TryDequeue(out sender))
            {
                sender.IsCompleted = true;
                var senderl= sender as FiberThreadAwaiter<T>;
                return senderl;
            }
            else
                return null;
        }

     

        public FiberThreadAwaiter<T> Read<T>()
        {
            Type key = typeof(T);

            if (!senders.ContainsKey(key) || senders[key].Count == 0)
            {
                var GhostThread = Fiber.Current;



                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = FiberThreadAwaiter<T>.New(GhostThread);

                if (receivers.ContainsKey(key))
                {
                    receivers[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
                else
                {
                    ConcurrentQueue<FiberThreadAwaiterBase> table = new ConcurrentQueue<FiberThreadAwaiterBase>();
                    receivers.AddOrUpdate(key, table, (a, b) => table);
                    receivers[key].Enqueue(waitingGhostThread);
                    return waitingGhostThread;
                }
            }

            FiberThreadAwaiterBase sender;

            if (senders[key].TryPeek(out sender))
            {
                sender.IsCompleted = true;
                var senderl = sender as FiberThreadAwaiter<T>;
                return senderl;
            }
            else
                return null;
        }

        public FiberThreadAwaiter<T> Back<T>()
        {
            Type key = typeof(T);

            if (!senders.ContainsKey(key) || senders[key].Count == 0)
            {
                var GhostThread = Fiber.Current;



                if (GhostThread == null)
                    GhostThread = this;

                var waitingGhostThread = FiberThreadAwaiter<T>.New(GhostThread);

                if (receivers.ContainsKey(key))
                {
                    receivers[key].Enqueue(waitingGhostThread);
                    
                }
                else
                {
                    ConcurrentQueue<FiberThreadAwaiterBase> table = new ConcurrentQueue<FiberThreadAwaiterBase>();
                    receivers.AddOrUpdate(key, table, (a, b) => table);
                    receivers[key].Enqueue(waitingGhostThread);
                   
                }
            }

            FiberThreadAwaiterBase sender;

            if (senders[key].TryDequeue(out sender))
            {
                sender.IsCompleted = true;

                var previousSyncContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
                if(sender.Continuation!=null)
                    sender.Continuation();
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);
               
                var senderl = sender as FiberThreadAwaiter<T>;
                return senderl;
            }
            else
                return null;
        }

        public FiberThreadAwaiter<T> Send<T>(T data)
        {
            Type key = typeof(T);


            // Nobody receiving, let's wait until something comes up
            var GhostThread = Fiber.Current;

            if (GhostThread == null)
                GhostThread = this;


            var waitingGhostThread = FiberThreadAwaiter<T>.New(GhostThread);
            waitingGhostThread.Result = data;

            if (senders.ContainsKey(key))
            {
                senders[key].Enqueue(waitingGhostThread);
            }
            else
            {
                ConcurrentQueue<FiberThreadAwaiterBase> table = new ConcurrentQueue<FiberThreadAwaiterBase>();
                senders.AddOrUpdate(key, table, (a, b) => table);
                senders[key].Enqueue(waitingGhostThread);
            }


            return waitingGhostThread;


        }

    }
}
#endif