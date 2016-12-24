#if!Net2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYSocket.ZYCoroutinesin
{
    public class FiberSynchronizationContext : SynchronizationContext
    {
        public Fiber fiber { get; set; }

        public FiberSynchronizationContext(Fiber fiber)
        {
            this.fiber = fiber;
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // There is two case:
            // 1/ We are either in normal MicroThread inside Scheduler.Step() (CurrentThread test),
            // in which case we will directly execute the callback to avoid further processing from scheduler.
            // Also, note that Wait() sends us event that are supposed to come back into scheduler.
            // Note: As it will end up on the callstack, it might be better to Schedule it instead (to avoid overflow)?
            // 2/ Otherwise, we just received an external task continuation (i.e. TaskEx.Sleep()), or a microthread triggering another,
            // so schedule it so that it comes back in our regular scheduler.
            if (Fiber.Current == fiber)
            {
                d(state);
            }
            else if (fiber.IsOver)
            {
                throw new InvalidOperationException("fiber is already completed but still posting continuations.");
            }
            else
            {

                SynchronizationContext.SetSynchronizationContext(this);
                d(state);
                // throw new InvalidOperationException("The fiber is not original ");
            }
        }
    }
}
#endif