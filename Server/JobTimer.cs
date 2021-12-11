using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int execTick;
        public Action action;

        public int CompareTo(JobTimerElement other)
        {
            return other.execTick - execTick;
        }
    }

    class JobTimer
    {
        private readonly PriorityQueue<JobTimerElement> _priorityQueue = new();
        private readonly object _lock = new();

        public static JobTimer Instance { get; } = new();
        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElement job;
            job.execTick = Environment.TickCount + tickAfter;
            job.action = action;

            lock (_lock)
            {
                _priorityQueue.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                var now = System.Environment.TickCount;
                JobTimerElement job;

                lock (_lock)
                {
                    if (_priorityQueue.Count == 0)
                    {
                        break;
                    }

                    job = _priorityQueue.Peek();
                    if (job.execTick > now)
                    {
                        break;
                    }

                    _priorityQueue.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
