using System.Threading;

namespace ZYSocket.MicroThreading
{
    public class MicrothreadProxySynchronizationContext : SynchronizationContext, IMicroThreadSynchronizationContext
    {
        private readonly MicroThread microThread;

        public MicrothreadProxySynchronizationContext(MicroThread microThread)
        {
            this.microThread = microThread;
        }

        MicroThread IMicroThreadSynchronizationContext.MicroThread => microThread;
    }
}