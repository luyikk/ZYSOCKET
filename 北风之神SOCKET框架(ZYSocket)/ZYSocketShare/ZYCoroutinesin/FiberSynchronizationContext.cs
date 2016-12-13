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
    }
}
