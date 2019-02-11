using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ZuneLike
{
    public static class AsyncHelper
    {
        public static void RunSync(Func<Task> task, object p)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.Complete();
                }
            }, null);
            synch.Start();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.Complete();
                }
            }, null);
            synch.Start();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            private bool done;
            public Exception InnerException { get; set; }
            private readonly AutoResetEvent mux = new AutoResetEvent(false);
            private readonly ConcurrentQueue<Tuple<SendOrPostCallback, object>> tasks =
                new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Use Post instead.");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                tasks.Enqueue(Tuple.Create(d, state));
                mux.Set();
            }

            public void Complete()
            {
                Post(_ => done = true, null);
            }

            public void Start()
            {
                while (!done)
                {
                    if (tasks.TryDequeue(out var task))
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null)
                        {
                            throw new AggregateException(InnerException);
                        }
                    }
                    else
                    {
                        mux.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }

            public void Dispose()
            {
                if (mux != null)
                {
                    mux.Dispose();
                }
            }
        }
    }
}
