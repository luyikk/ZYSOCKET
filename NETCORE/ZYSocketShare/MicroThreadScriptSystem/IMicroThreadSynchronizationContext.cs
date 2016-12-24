#if!Net2
namespace ZYSocket.MicroThreading
{
    internal interface IMicroThreadSynchronizationContext
    {
        MicroThread MicroThread { get; }
    }
}
#endif