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
            }
            else if (RunTask.Status == TaskStatus.RanToCompletion)
            {
                RunTask.Dispose();
                RunTask = Task.Factory.StartNew(Run);
            }
            else if (RunTask.Status == TaskStatus.Faulted)
            {
                CheckError();

                RunTask.Dispose();
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
