using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ZYSocket.ZYCoroutinesin
{

    public enum FiberStateEnum
    {
        FiberCreated,
        FiberRunning,
        FiberStopPending,
        FiberStopped
    };

    public static class RiberStatic
    {
        [DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ConvertThreadToFiberEx(int pvParam, int dwFlags);


        [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
        public delegate void CallbackDelegate(IntPtr arg);

        [DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateFiberEx(
            uint dwStackCommitSize,     // 堆栈初始提交的大小
            uint dwStackReserveSize,    // 需要保留的虚拟内存的大小
            int dwFlags,     // 创建旗标
            CallbackDelegate pStartAddress,     // 纤程函数指针
            IntPtr pvParam);    // 传递给纤程函数的参数

        [DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall)]
        public static extern void DeleteFiber(IntPtr pvFiberExecutionContext);

        [DllImport("KERNEL32.DLL", CallingConvention = CallingConvention.StdCall)]
        public static extern void SwitchToFiber(IntPtr pvFiberExecutionContext);

    }



    public class Coroutinesin<SET, RES>
    {
        static void CorSwitchToFiber(IntPtr fiber)
        {
            RiberStatic.SwitchToFiber(fiber);
        }
        static void unmanaged_fiberproc(IntPtr arg)
        {

            GCHandle g = GCHandle.FromIntPtr(arg);
            Coroutinesin<SET, RES> tmp = g.Target as Coroutinesin<SET, RES>;
            g.Free();

            tmp.ExtIng();
            
            RiberStatic.SwitchToFiber(FiberA);
        }

        private void ExtIng()
        {
            state = FiberStateEnum.FiberRunning;
            try
            {

                Run(retvalSet);

            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
            }

            state = FiberStateEnum.FiberStopped;
            retvalSet = SetDefautValue;
            retvalRes = ResDefautValue;
        }


        [ThreadStatic]
        protected static IntPtr FiberA;

        private static RiberStatic.CallbackDelegate fiberproc = new RiberStatic.CallbackDelegate(unmanaged_fiberproc);

        protected IntPtr FiberB { get; set; }


        protected FiberStateEnum state { get; set; }

        public SET SetDefautValue
        {
            get;
            set;
        }
        public RES ResDefautValue
        {
            get;
            set;
        }



        public SET retvalSet { get; set; }

        public RES retvalRes { get; set; }


        [ThreadStatic]
        static bool thread_is_fiber;



        static object Lockobj = new object();


        int ThreadId
        {
            get;
            set;
        }

        public virtual void Run(SET arg)
        {


        }


        protected SET yield(RES obj)
        {

            retvalRes = obj;
            CorSwitchToFiber(FiberB);
            CorSwitchToFiber(FiberA);
         
            if (state == FiberStateEnum.FiberStopPending)
                throw new Exception("StopFiber");
            return retvalSet;

        }


        protected RES Resume()
        {

            return Resume(SetDefautValue);

        }

        protected RES Resume(SET obj)
        {

            if (FiberB == IntPtr.Zero || state == FiberStateEnum.FiberStopped)
                return ResDefautValue;

            initialize_thread();
            retvalSet = obj;
            CorSwitchToFiber(FiberA);
            CorSwitchToFiber(FiberB);
        
            return retvalRes;

        }


        void initialize_thread()
        {
            if (ThreadId != 0 && ThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
                throw new Exception("The Thread Not Use It");

            if (!thread_is_fiber)
            {
                if (FiberA == IntPtr.Zero)
                {
                    FiberA = RiberStatic.ConvertThreadToFiberEx(0, 1);

                    if (FiberA == IntPtr.Zero)
                    {
                        throw new Exception("not ConvertThreadToFiber");
                    }
                }


                thread_is_fiber = true;
            }

        }


        public Coroutinesin(SET defautSetValue, RES defautResValue)
        {

            ResDefautValue = defautResValue;
            SetDefautValue = defautSetValue;
            var ptr = GCHandle.ToIntPtr(GCHandle.Alloc(this));
            FiberB = RiberStatic.CreateFiberEx(0, 0, 1, fiberproc, ptr);
            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        }

        private bool IsDispose;

        public virtual void Dispose()
        {
            if (!IsDispose)
            {
                IsDispose = true;

                if (FiberB != IntPtr.Zero)
                {
                    RiberStatic.DeleteFiber(FiberB);
                    FiberB = IntPtr.Zero;
                }
            }


            state = FiberStateEnum.FiberStopped;
        }

    }
}
