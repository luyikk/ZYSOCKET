using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZYSocket.ZYCoroutinesin
{
    public class FiberManager<TYPE, SET, RES> where TYPE : Fiber<SET, RES>, new()
    {
        #region 全局静态唯一对象
        static object lockthis = new object();

        static FiberManager<TYPE, SET, RES> _My;

        public static FiberManager<TYPE, SET, RES> GetInstance()
        {
            lock (lockthis)
            {


                if (_My == null)
                    _My = new FiberManager<TYPE, SET, RES>();
            }

            return _My;
        }
        
        #endregion



        private List<Fiber<SET, RES>> Fiber;

        private Queue<CreateFiberCmd<TYPE>> CreateQueue;

        public Thread thread { get; private set; }


        public void Close()
        {
            for (int i = 0; i < Fiber.Count; i++)
            {
                Fiber[i].Dispose();
            }

            Fiber.Clear();

            foreach (var item in CreateQueue)
            {
                item.WaitHandle.Set();
            }

            CreateQueue.Clear();

            thread.Abort();
        }

        private FiberManager()
        {
            CreateQueue = new Queue<CreateFiberCmd<TYPE>>();
            Fiber = new List<Fiber<SET, RES>>();
            thread = new Thread((Run), 0x800000);
            thread.Start();
        }


        public TYPE CreateFiber()
        {
            CreateFiberCmd<TYPE> tmp = new CreateFiberCmd<TYPE>();
            tmp.WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            tmp.Value = null;
            CreateQueue.Enqueue(tmp);

            tmp.WaitHandle.WaitOne();
            tmp.WaitHandle.Close();

            return tmp.Value;

        }



        private void Run()
        {
            bool isSleep = false;

            while (true)
            {

                while (CreateQueue.Count > 0)
                {
                    var create = CreateQueue.Dequeue();
                    TYPE tmp = new TYPE();
                    Fiber.Add(tmp);
                    create.Value = tmp;
                    create.WaitHandle.Set();

                }

                if (!isSleep && Fiber.Count == 0)
                {
                    isSleep = true;
                }

                for (int i = 0; i < Fiber.Count; i++)
                {
                    var fiber = Fiber[i];

                    if (fiber == null)
                        return;

                    if (fiber.State != FiberFlag.Complete)
                    {
                        bool isruning = fiber.ThreadRun();

                        if (!isruning && !isSleep)
                        {
                            isSleep = true;
                        }
                    }
                    else
                    {
                        fiber.Dispose();
                        Fiber.Remove(fiber);
                        break;
                    }
                }



                if (isSleep)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(0);
                }


                isSleep = false;
            }
        }


        class CreateFiberCmd<T>
        {

            public EventWaitHandle WaitHandle { get; set; }

            public T Value { get; set; }
        }

    }

}
