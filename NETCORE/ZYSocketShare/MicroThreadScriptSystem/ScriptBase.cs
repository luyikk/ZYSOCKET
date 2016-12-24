#if!Net2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYSocket.MicroThreading
{

    public abstract class Script
    {
       
        public int Priority { get; set; }


        public virtual void Cancel()
        {
        }

        protected internal virtual void PriorityUpdated()
        {
        }
    }

    public abstract class StartupScript: Script
    {
      

        /// <summary>
        /// Called before the script enters it's update loop.
        /// </summary>
        public virtual void Start()
        {
        }
    }

    public abstract class SyncScript : StartupScript
    {
        internal PriorityQueueNode<SchedulerEntry> UpdateSchedulerNode;

        protected Scheduler SystemCore => Scheduler.Current;

        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void Update();
    }


    public abstract class AsyncScript : Script
    {

        internal MicroThread MicroThread { get; set; }


        internal CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets a token indicating if the script execution was canceled.
        /// </summary>
        public CancellationToken CancellationToken => MicroThread.CancellationToken;


        protected Scheduler SystemCore => Scheduler.Current;


        /// <summary>
        /// Called once, as a microthread
        /// </summary>
        /// <returns></returns>
        public abstract Task Execute();

        protected internal override void PriorityUpdated()
        {          

            // Update micro thread priority
            if (MicroThread != null)
                MicroThread.Priority = Priority;
        }
    }
}
#endif