using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _releaser;

        public AsyncLock()
        {
            _releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ? _releaser :
                        wait.ContinueWith((t, o) => (IDisposable)o,
                        _releaser.Result,
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _lock;
            internal Releaser(AsyncLock theLock) { _lock = theLock; }
            public void Dispose() { _lock._semaphore.Release(); }
        }
    }
}