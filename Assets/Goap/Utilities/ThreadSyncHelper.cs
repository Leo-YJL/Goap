//------------------------------------------------------------
// Author: 烟雨迷离半世殇
// Mail: 1778139321@qq.com
// Data: 2020年4月28日 16:27:06
//------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ReGoap.Utilities
{
    public class OneThreadSynchronizationContext : SynchronizationContext
    {
        public static OneThreadSynchronizationContext Instance { get; } = new OneThreadSynchronizationContext();

        private readonly int mainThreadId = Thread.CurrentThread.ManagedThreadId;

        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        private Action a;

        public void Update()
        {
            while (true)
            {
                if (!this.queue.TryDequeue(out a))
                {
                    return;
                }

                a();
            }
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.mainThreadId)
            {
                callback(state);
                return;
            }

            this.queue.Enqueue(() => { callback(state); });
        }
    }
}