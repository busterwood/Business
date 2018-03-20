using BusterWood.Collections;
using BusterWood.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SampleTests
{
    interface IBusinessProcess
    {
        object SyncRoot { get; }
        void Run();
    }

    //TODO: this looks a lots like a task engine
    class BusinessProcessScheduler : IBusinessProcessScheduler
    {
        //queue
        readonly Dictionary<object, IBusinessProcess> _running = new Dictionary<object, IBusinessProcess>();
        readonly Dictionary<object, RunQueue> _runQueue = new Dictionary<object, RunQueue>();

        /// <summary>Run now if we can, otherwise add to the run queue.</summary>
        /// <param name="businessProcess"></param>
        /// <param name="cancellationToken">A way to cancel </param>
        /// <returns>TRUE if execution will be immediate, FALSE if execution is delayed to an another process</returns>
        public bool Schedule(IBusinessProcess businessProcess, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool blocked;
            lock (_running)
            {

                blocked = _running.ContainsKey(businessProcess.SyncRoot);
                if (!blocked)
                {
                    _running.Add(businessProcess.SyncRoot, businessProcess);
                }
                else
                {
                    var q = _runQueue.GetOrAdd(businessProcess.SyncRoot, _ => new RunQueue());
                    q.Enqueue(businessProcess, cancellationToken);
                }
            }

            if (blocked)
                return false;

            RunCore(businessProcess);
            return true;
        }

        private void RunCore(IBusinessProcess businessProcess)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    businessProcess.Run();
                }
                catch (Exception e)
                {
                    Log.Error($"{businessProcess} failed with {e}");
                    // TODO: restart with backoff when technical failure?
                }
                finally
                {
                    IBusinessProcess next = OnRunFinished(businessProcess);
                    if (next != null)
                    {
                        RunCore(next); // schedules next item outside of lock
                    }
                }
            });
        }

        private IBusinessProcess OnRunFinished(IBusinessProcess businessProcess)
        {
            lock (_running)
            {
                var q = _runQueue.GetValueOrDefault(businessProcess.SyncRoot);
                var next = q?.Dequeue();
                if (next == null)
                    _running.Remove(businessProcess.SyncRoot);
                else
                    _running[next.SyncRoot] = next; // replace existing
               return next;
            }
        }

        class RunQueue
        {
            readonly Queue<QueueItem> waiting = new Queue<QueueItem>();

            public void Enqueue(IBusinessProcess businessProcess, CancellationToken cancellationToken)
            {
                waiting.Enqueue(new QueueItem(businessProcess, cancellationToken));
            }

            public IBusinessProcess Dequeue()
            {
                for(;;)
                {
                    if (waiting.Count == 0)
                        return null;

                    var next = waiting.Dequeue();
                    if (next.cancellationToken.IsCancellationRequested)
                        continue;

                    return next.businessProcess;
                }
            }

            struct QueueItem
            {
                public readonly IBusinessProcess businessProcess;
                public readonly CancellationToken cancellationToken;

                public QueueItem(IBusinessProcess businessProcess, CancellationToken cancellationToken)
                {
                    this.businessProcess = businessProcess;
                    this.cancellationToken = cancellationToken;
                }
            }
        }
    }
}
