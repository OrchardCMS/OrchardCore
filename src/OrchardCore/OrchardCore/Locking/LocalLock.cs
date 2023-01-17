using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Locking
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire named locks for a given tenant.
    /// </summary>
    public class LocalLock : IDistributedLock, ILocalLock, IDisposable
    {
        private readonly ILogger _logger;

        private readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

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
            var semaphore = _asyncKeyedLocker.GetOrAdd(key);
            await semaphore.SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            return new Locker(semaphore, expiration);
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            var semaphore = _asyncKeyedLocker.GetOrAdd(key);

            if (await semaphore.SemaphoreSlim.WaitAsync(timeout != TimeSpan.MaxValue ? timeout : Timeout.InfiniteTimeSpan).ConfigureAwait(false))
            {
                return (new Locker(semaphore, expiration), true);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Timeout elapsed before acquiring the named lock '{LockName}' after the given timeout of '{Timeout}'.",
                    key, timeout.ToString());
            }

            return (null, false);
        }

        public Task<bool> IsLockAcquiredAsync(string key)
        {
            return Task.FromResult(_asyncKeyedLocker.IsInUse(key));
        }

        private class Semaphore
        {
            public Semaphore(string key, SemaphoreSlim value)
            {
                Key = key;
                Value = value;
                RefCount = 1;
            }

            internal string Key { get; }
            internal SemaphoreSlim Value { get; }
            internal int RefCount { get; set; }
        }

        private class Locker : ILocker
        {
            private readonly AsyncKeyedLockReleaser<string> _semaphore;
            private readonly CancellationTokenSource _cts;
            private bool _disposed;

            public Locker(AsyncKeyedLockReleaser<string> semaphore, TimeSpan? expiration)
            {
                _semaphore = semaphore;

                if (expiration.HasValue && expiration.Value != TimeSpan.MaxValue)
                {
                    _cts = new CancellationTokenSource(expiration.Value);
                    _cts.Token.Register(Release);
                }
            }

            private void Release()
            {
                _semaphore.Dispose();
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
            var semaphores = _asyncKeyedLocker.Index.Values.ToArray();

            foreach (var semaphore in semaphores)
            {
                semaphore.SemaphoreSlim.Dispose();
            }
        }
    }
}
