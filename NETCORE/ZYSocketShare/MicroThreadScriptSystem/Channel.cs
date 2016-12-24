#if!Net2
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ZYSocket.MicroThreading
{
    // TODO: Thread-safety
    /// <summary>
    /// Provides a communication mechanism between <see cref="MicroThread"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="MicroThread"/> can send and receive to a <see cref="Channel"/>. Depending on the <see cref="Channel.Preference"/>,
    /// sending or receiving <see cref="MicroThread"/> might be suspended and yield execution to another <see cref="MicroThread"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class Channel
    {
        ConcurrentDictionary<Type, ConcurrentQueue<ChannelMicroThreadAwaiterBase>> receivers = new ConcurrentDictionary<Type, ConcurrentQueue<ChannelMicroThreadAwaiterBase>>();

        ConcurrentDictionary<Type, ConcurrentQueue<ChannelMicroThreadAwaiterBase>> senders = new ConcurrentDictionary<Type, ConcurrentQueue<ChannelMicroThreadAwaiterBase>>();

        public Channel()
        {
            Preference = ChannelPreference.PreferReceiver;
        }

        /// <summary>
        /// Gets or sets the preference, allowing you to customize how <see cref="Send"/> and <see cref="Receive"/> behave regarding scheduling.
        /// </summary>
        /// <value>
        /// The preference.
        /// </value>
        public ChannelPreference Preference { get; set; }

        /// <summary>
        /// Gets the balance, which is the number of <see cref="MicroThread"/> waiting to send (if greater than 0) or receive (if smaller than 0).
        /// </summary>
        /// <value>
        /// The balance.
        /// </value>
        public int Balance<T>()
        {
            Type key = typeof(T);

            if(senders.ContainsKey(key))
            {
                if(receivers.ContainsKey(key))
                {
                    return senders[key].Count - receivers[key].Count;
                }
                else
                {
                    return senders[key].Count - 0;
                }
            }
            else
            {
                if (receivers.ContainsKey(key))
                {
                    return 0 - receivers[key].Count;
                }
                else
                {
                    return 0;
                }
            }

            

        }


        public  void SetSync<T>(T data)
        {
             Set<T>(data);
        }



        /// <summary>
        /// Sends a value over the channel. If no other <see cref="MicroThread"/> is waiting for data, the sender will be blocked.
        /// If someone was waiting for data, which of the sender or receiver continues next depends on <see cref="Preference"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Awaitable data.</returns>
        public ChannelMicroThreadAwaiter<T> Set<T>(T data)
        {

            Type key = typeof(T);

            if (!receivers.ContainsKey(key) || (receivers.ContainsKey(key) && receivers[key].Count == 0))
            {
                // Nobody receiving, let's wait until something comes up
                var microThread = MicroThread.Current;
                var waitingMicroThread = ChannelMicroThreadAwaiter<T>.New(microThread);
                waitingMicroThread.Result = data;

                if (senders.ContainsKey(key))
                {
                    senders[key].Enqueue(waitingMicroThread);
                    return waitingMicroThread;
                }
                else
                {
                    ConcurrentQueue<ChannelMicroThreadAwaiterBase> tmp = new ConcurrentQueue<ChannelMicroThreadAwaiterBase>();
                    senders.AddOrUpdate(key, tmp, (a, b) => tmp);
                    senders[key].Enqueue(waitingMicroThread);
                    return waitingMicroThread;
                }


            }

            ChannelMicroThreadAwaiterBase receiverbase;

            if (receivers[key].TryDequeue(out receiverbase))
            {
                var receiver = receiverbase as ChannelMicroThreadAwaiter<T>;
                receiver.Result = data;
                if (Preference == ChannelPreference.PreferSender)
                {
                    receiver.MicroThread.ScheduleContinuation(ScheduleMode.Last, receiver.Continuation);
                }
                else if (Preference == ChannelPreference.PreferReceiver)
                {
                    receiver.MicroThread.ScheduleContinuation(ScheduleMode.First, receiver.Continuation);
                    // throw new NotImplementedException();
                    //await Scheduler.Yield();
                }
                receiver.IsCompleted = true;
                return receiver;
            }
            else
                return null;
        }

        /// <summary>
        /// Receives a value over the channel. If no other <see cref="MicroThread"/> is sending data, the receiver will be blocked.
        /// If someone was sending data, which of the sender or receiver continues next depends on <see cref="Preference"/>.
        /// </summary>
        /// <returns>Awaitable data.</returns>
        public ChannelMicroThreadAwaiter<T> Get<T>()
        {
            Type key = typeof(T);


            if (!senders.ContainsKey(key) || (senders.ContainsKey(key) && senders[key].Count == 0))
            {
                var microThread = MicroThread.Current;
                if (microThread == null)
                    throw new Exception("Cannot receive out of micro-thread context.");

                var waitingMicroThread = ChannelMicroThreadAwaiter<T>.New(microThread);

                if (receivers.ContainsKey(key))
                {
                    receivers[key].Enqueue(waitingMicroThread);
                    return waitingMicroThread;
                }
                else
                {
                    ConcurrentQueue<ChannelMicroThreadAwaiterBase> tmp = new ConcurrentQueue<ChannelMicroThreadAwaiterBase>();
                    receivers.AddOrUpdate(key, tmp, (a, b) => tmp);
                    receivers[key].Enqueue(waitingMicroThread);
                    return waitingMicroThread;
                }
            }


            ChannelMicroThreadAwaiterBase sender;

            if (senders[key].TryDequeue(out sender))
            {
                if (Preference == ChannelPreference.PreferReceiver)
                {
                    sender.MicroThread.ScheduleContinuation(ScheduleMode.Last, sender.Continuation);
                }
                else if (Preference == ChannelPreference.PreferSender)
                {
                    sender.MicroThread.ScheduleContinuation(ScheduleMode.First, sender.Continuation);

                }
                sender.IsCompleted = true;
                return sender as ChannelMicroThreadAwaiter<T>;
            }
            else
            {
                return null;
            }
        }




    }
}
#endif