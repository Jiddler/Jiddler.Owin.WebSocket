using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jiddler.Owin.WebSocket.Extensions {
    // Allows serial queuing of Task instances
    // The tasks are not called on the current synchronization context
    public sealed class TaskQueue {
        private readonly object _lockObj = new object();
        private Task _lastQueuedTask;
        private volatile bool _drained;
        private int? _maxSize;
        private int _size;

        /// <summary>
        /// Current size of the queue depth
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// Maximum size of the queue depth.  Null = unlimited
        /// </summary>
        public int? MaxSize => _maxSize;

        public TaskQueue() : this(TaskAsyncHelper.Empty) {
        }

        public TaskQueue(Task initialTask) {
            _lastQueuedTask = initialTask;
        }

        /// <summary>
        /// Set the maximum size of the Task Queue chained operations.  
        /// When pending send operations limits reached a null Task will be returned from Enqueue
        /// </summary>
        /// <param name="maxSize">Maximum size of the queue</param>
        public void SetMaxQueueSize(int? maxSize) {
            _maxSize = maxSize;
        }

        /// <summary>
        /// Enqueue a new task on the end of the queue
        /// </summary>
        /// <returns>The enqueued Task or NULL if the max size of the queue was reached</returns>
        public Task Enqueue<T>(Func<T, Task> taskFunc, T state) {
            // Lock the object for as short amount of time as possible
            lock (_lockObj) {
                if (_drained) {
                    return _lastQueuedTask;
                }

                Interlocked.Increment(ref _size);

                if (_maxSize != null) {
                    // Increment the size if the queue
                    if (_size > _maxSize) {
                        Interlocked.Decrement(ref _size);

                        // We failed to enqueue because the size limit was reached
                        return null;
                    }
                }

                var newTask = _lastQueuedTask.Then((next, nextState) => {
                        return next(nextState).Finally(s => {
                                var queue = (TaskQueue) s;
                                Interlocked.Decrement(ref queue._size);
                            },
                            this);
                    },
                    taskFunc, state);

                _lastQueuedTask = newTask;
                return newTask;
            }
        }

        /// <summary>
        /// Triggers a drain fo the task queue and blocks until the drain completes
        /// </summary>
        public void Drain() {
            lock (_lockObj) {
                _drained = true;

                _lastQueuedTask.Wait();

                _drained = false;
            }
        }
    }
}