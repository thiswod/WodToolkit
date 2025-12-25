using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace WodToolKit.src.Thread
{
    /// <summary>
    /// 简单线程池类，用于管理多个工作线程执行任务队列中的任务。
    /// </summary>
    public class SimpleThreadPool : IDisposable
    {
        private readonly int _workerCount;
        private readonly Queue<Action> _taskQueue = new Queue<Action>();
        private readonly System.Threading.Thread[] _workers;
        private readonly object _locker = new object();
        private bool _isDisposed;
        private int _pendingTasks = 0; // 跟踪未完成任务的数量
        /// <summary>
        /// 初始化 SimpleThreadPool 类的新实例。
        /// </summary>
        /// <param name="workerCount">线程池中的工作线程数量</param>
        /// <exception cref="ArgumentOutOfRangeException">当 workerCount 小于或等于 0 时抛出</exception>
        public SimpleThreadPool(int workerCount)
        {
            if (workerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(workerCount), "Worker count must be positive");

            _workerCount = workerCount;
            _workers = new System.Threading.Thread[_workerCount];
            
            // 创建并启动工作线程
            for (int i = 0; i < _workerCount; i++)
            {
                _workers[i] = new System.Threading.Thread(Worker) { IsBackground = true };
                _workers[i].Start();
            }
        }
        /// <summary>
        /// 将任务添加到线程池的任务队列中。
        /// </summary>
        /// <param name="task">要执行的任务</param>
        /// <exception cref="ObjectDisposedException">当线程池已被释放时抛出</exception>
        /// <exception cref="ArgumentNullException">当任务为 null 时抛出</exception>
        public void QueueTask(Action task)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("SimpleThreadPool");

            if (task == null)
                throw new ArgumentNullException(nameof(task));

            lock (_locker)
            {
                _taskQueue.Enqueue(task);
                _pendingTasks++;
                Monitor.Pulse(_locker); // 唤醒一个等待的线程
            }
        }

        private void Worker()
        {
            while (true)
            {
                Action task;
                lock (_locker)
                {
                    // 等待队列中有任务
                    while (_taskQueue.Count == 0 && !_isDisposed)
                    {
                        Monitor.Wait(_locker);
                    }

                    // 检查是否需要退出线程
                    if (_isDisposed && _taskQueue.Count == 0)
                        return;

                    task = _taskQueue.Dequeue();
                }

                try
                {
                    task.Invoke();
                }
                catch (Exception ex)
                {
                    // 处理任务执行中的异常
                    Debug.WriteLine($"Task execution error: {ex.Message}");
                }
                finally
                {
                    // 无论任务成功或失败，都减少未完成任务计数
                    lock (_locker)
                    {
                        _pendingTasks--;
                        // 如果没有任务了，通知等待中的线程
                        if (_pendingTasks == 0)
                        {
                            Monitor.PulseAll(_locker);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 等待所有任务完成。
        /// </summary>
        /// <exception cref="ObjectDisposedException">当线程池已被释放时抛出</exception>
        /// <exception cref="InvalidOperationException">当在等待完成时调用了 QueueTask 方法时抛出</exception>
        public void Wait()
        {
            lock (_locker)
            {
                // 当还有未完成任务时，等待
                while (_pendingTasks > 0)
                {
                    Monitor.Wait(_locker);
                }
            }
        }
        /// <summary>
        /// 释放线程池占用的所有资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                lock (_locker)
                {
                    _isDisposed = true;
                    Monitor.PulseAll(_locker); // 唤醒所有线程以便它们可以退出
                }

                // 等待所有工作线程完成
                foreach (var worker in _workers)
                {
                    worker.Join();
                }
            }
        }

        ~SimpleThreadPool()
        {
            Dispose(false);
        }
    }
}
