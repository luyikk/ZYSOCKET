namespace ZYSocket.MicroThreading
{
    /// <summary>
    /// Represents a node in a priority queue, to allow O(n) removal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueueNode<T>
    {
        public T Value;

        public int Index { get; internal set; }

        public PriorityQueueNode(T value)
        {
            Value = value;
            Index = -1;
        }
    }
}