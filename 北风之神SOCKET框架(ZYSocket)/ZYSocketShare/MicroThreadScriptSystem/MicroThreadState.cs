namespace ZYSocket.MicroThreading
{
    public enum MicroThreadState : int
    {
        None,
        Starting,
        Running,
        Completed,
        Canceled,
        Failed,
    }
}