using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ZYSocket.EnsureSend
{

    public class TaskQueue<T>
    {

        static TaskQueue()
        {
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                //设置所有未觉察异常被觉察
                e.SetObserved();
            };

            StopExcption = new ConcurrentBag<Type>();
        }

        public static ConcurrentBag<Type> StopExcption
        {
            get; set;
        }



        public Task RunTask { get; private set; }


        public ConcurrentQueue<ActionRun<T>> ActionQueue
        {
            get; private set;
        }

        public TaskQueue()
        {
            ActionQueue = new ConcurrentQueue<ActionRun<T>>();
            RunTask = new Task(Run);
        }



        private void Run()
        {
            while (ActionQueue.Count >0)
            {
                ActionRun<T> actionRun;
                if (ActionQueue.TryDequeue(out actionRun))
                {
                    if (actionRun.Run())
                        for (int i = 0; i < ActionQueue.Count; i++)
                            ActionQueue.TryDequeue(out actionRun);
                }
                else
                    break;
            }

        }

        public void CheckError()
        {
            if (RunTask==null||RunTask.Exception == null)
                return;

            foreach (var exp in RunTask.Exception.InnerExceptions)
            {
                if (StopExcption.Contains(exp.GetType()))
                {
                    throw exp;
                }
            }
        }


        public void Push(ActionRun<T> item)
        {
                           

            ActionQueue.Enqueue(item);

            if (RunTask.Status == TaskStatus.Created)
            {
                RunTask.Start();
                RunTask.ContinueWith(task =>
                {
                    var ae = task.Exception;

                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else if (RunTask.Status == TaskStatus.RanToCompletion)
            {
#if !COREFX
                RunTask.Dispose();
#endif
                RunTask = Task.Factory.StartNew(Run);
            }
            else if (RunTask.Status == TaskStatus.Faulted)
            {
                CheckError();
#if !COREFX
                RunTask.Dispose();
#endif
                RunTask = Task.Factory.StartNew(Run);

            }

        }
        

    }

    public class ActionRun<C>
    {
        public Func<C,bool> action { get; set; }

        public C Obj { get; set; }

        public ActionRun(Func<C, bool> act, C obj)
        {
            this.action = act;
            this.Obj = obj;
        }

        public bool Run()
        {
           return action.Invoke(Obj);
        }

    }
}
