using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Boredbone.Utility
{
    public sealed class AsyncLock
    {
        private readonly object gate = new object();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly AsyncLocal<int> recursionCount = new AsyncLocal<int>();

        public Task<Releaser> LockAsync()
        {
            var shouldAcquire = false;

            lock (gate)
            {
                if (recursionCount.Value == 0)
                {
                    shouldAcquire = true;
                    recursionCount.Value = 1;
                }
                else
                {
                    recursionCount.Value++;
                }
            }

            if (shouldAcquire)
            {
                return semaphore.WaitAsync().ContinueWith(_ => new Releaser(this));
            }

            return Task.FromResult(new Releaser(this));
        }

        private void Release()
        {
            lock (gate)
            {
                Debug.Assert(recursionCount.Value > 0);

                if (--recursionCount.Value == 0)
                {
                    semaphore.Release();
                }
            }
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock parent;

            public Releaser(AsyncLock parent) => this.parent = parent;

            public void Dispose() => parent.Release();
        }
    }
}
