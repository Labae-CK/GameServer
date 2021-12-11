using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        private readonly Queue<Action> _jobQueue = new();
        private readonly object _lock = new();
        private bool _flush;

        public void Push(Action job)
        {
            var flush = false;

            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                {
                    flush = _flush = true;
                }
            }

            if (flush)
            {
                Flush();
            }
        }

        private void Flush()
        {
            while (true)
            {
                var action = Pop();
                if (action == null)
                {
                    return;
                }

                action.Invoke();
            }
        }

        private Action Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count != 0)
                {
                    return _jobQueue.Dequeue();
                }

                _flush = false;
                return null;
            }
        }
    }
}
