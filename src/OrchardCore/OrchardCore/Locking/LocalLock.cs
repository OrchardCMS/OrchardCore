using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Locking
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire named locks for a given tenant.
    /// </summary>
    public class LocalLock : IDistributedLock, IDisposable
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public LocalLock(ILogger<LocalLock> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            return new Locker(semaphore, expiration);
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));

            if (await semaphore.WaitAsync(timeout))
            {
                return (new Locker(semaphore, expiration), true);
            }

            _logger.LogWarning("Fails to acquire the named lock '{LockName}' after the given timeout of '{Timeout}'.",
                key, timeout.ToString());

            return (null, false);
        }

        private class Locker : ILocker
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly CancellationTokenSource _cts;
            private volatile int _released;
            private bool _disposed;

            public Locker(SemaphoreSlim semaphore, TimeSpan? expiration)
            {
                _semaphore = semaphore;

                if (expiration.HasValue)
                {
                    _cts = new CancellationTokenSource(expiration.Value);
                    _cts.Token.Register(Release);
                }
            }

            private void Release()
            {
                if (Interlocked.Exchange(ref _released, 1) == 0)
                {
                    _semaphore.Release();
                }
            }

            public ValueTask DisposeAsync()
            {
                Dispose();
                return default;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _cts?.Dispose();

                Release();
            }
        }

        public void Dispose()
        {
            var semaphores = _semaphores.Values.ToArray();

            foreach (var semaphore in semaphores)
            {
                semaphore.Dispose();
            }
        }
    }
}
