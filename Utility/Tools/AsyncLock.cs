using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Boredbone.Utility
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim semaphore;

        private readonly Task<IDisposable> releaser;
        public bool IsLocked { get; private set; }


        public AsyncLock(int semaphoreCount)
        {
            this.semaphore = new SemaphoreSlim(semaphoreCount, semaphoreCount);
            this.IsLocked = false;
            this.releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public AsyncLock() : this(1)
        {
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = this.semaphore.WaitAsync();

            if (wait.IsCompleted)
            {
                this.IsLocked = true;
                return this.releaser;
            }

            return wait.ContinueWith(
                (_, state) =>
                {
                    this.IsLocked = true;
                    return (IDisposable)state;
                },
                this.releaser.Result,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
                );
        }


        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock target;

            internal Releaser(AsyncLock obj)
            {
                this.target = obj;
            }

            public void Dispose()
            {
                this.target.IsLocked = false;
                this.target.semaphore.Release();
            }
        }

    }
}
