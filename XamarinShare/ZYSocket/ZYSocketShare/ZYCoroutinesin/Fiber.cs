using System;
using System.Collections.Generic;
using System.Text;


namespace ZYSocket.ZYCoroutinesin
{

    public enum FiberFlag
    {
        Runing,
        Complete,
    }



    public class Fiber<SET, RES> : Coroutinesin<SET, RES>
    {

        static object lockObj = new object();

        public FiberFlag State { get; private set; }

        public Guid GID { get; private set; }       

        private Queue<ResCallbackModel> SetQueue { get;set; }

        public Action<Fiber<SET, RES>, SET> Target { get; private set; }


        public static SET defset { get; set; }
        public static RES defres { get; set; }



        public static Fiber<SET, RES> CreateFiber(Action<Fiber<SET, RES>, SET> action)
        {
            var fiber= FiberManager<Fiber<SET, RES>, SET, RES>.GetInstance().CreateFiber();
            fiber.Invoke(action);
            return fiber;
        }

        public static Fiber<SET, RES> CreateFiber()
        {
            return FiberManager<Fiber<SET, RES>, SET, RES>.GetInstance().CreateFiber();
        }


        public static void CloseAllFiber()
        {
            FiberManager<Fiber<SET, RES>, SET, RES>.GetInstance().Close();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="setDefault">默认值设置SET</param>
        /// <param name="resDefault">默认值设置RES</param>
        public Fiber()
            : base(defset, defres)
        {
            GID = Guid.NewGuid();
            State = FiberFlag.Runing;
            SetQueue = new Queue<ResCallbackModel>();
        }


        public void Invoke(Action<Fiber<SET, RES>, SET> target)
        {
            this.Target = target;
        }

        public override void Dispose()
        {

            State = FiberFlag.Complete;
            lock (lockObj)
            {
                SetQueue.Clear();
            }
            base.Dispose();
        }


        public override void Run(SET arg)
        {
            if (Target != null)
                Target(this, arg);
        }



        public SET Give(RES obj)
        {
            return base.yield(obj);

        }

        public SET Give()
        {
            return base.yield(defres);
        }


        public bool Set()
        {           
            return Set(SetDefautValue, null);
        }


        public bool Set(SET value)
        {          
            return Set(value, null);
        }
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="callback">返回回调</param>
        /// <returns>true设置成功， false 此对象已经停止工作了</returns>
        public bool Set(SET value, Action<RES> callback)
        {
            lock (lockObj)
            {
                if (Target == null)
                    return false;

                if (State == FiberFlag.Complete)
                    return false;

                ResCallbackModel tmp = new ResCallbackModel()
                {
                    ResCallBack = callback,
                    value = value
                };



                SetQueue.Enqueue(tmp);


                return true;
            }
        }


        ResCallbackModel model;


        public bool ThreadRun()
        {

            lock (lockObj)
            {

                model = null;

                if (SetQueue.Count > 0)
                {
                    model = SetQueue.Peek();

                }
                else
                    return false;



                if (model != null)
                {
                    Console.WriteLine("Id:"+System.Threading.Thread.CurrentThread.ManagedThreadId);

                    RES res = Resume(model.value);

                    if (model.ResCallBack != null)
                    {
                        model.ResCallBack(res);
                    }


                    if (state == FiberStateEnum.FiberStopped)
                    {

                        SetQueue.Clear();
                        State = FiberFlag.Complete;
                    }

                    SetQueue.Dequeue();

                    return true;
                }
                SetQueue.Dequeue();
                return false;
            }

        }

                
            
    


        class ResCallbackModel
        {
            public Action<RES> ResCallBack { get; set; }

            public SET value { get; set; }
        }

    }
}
