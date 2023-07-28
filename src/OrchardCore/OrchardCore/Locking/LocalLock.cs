using System;
using System.Collections.Generic;
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
    public sealed class LocalLock : IDistributedLock, ILocalLock, IDisposable
    {
        private readonly ILogger _logger;

        private readonly Dictionary<string, Semaphore> _semaphores = new();

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
            var semaphore = GetOrCreateSemaphore(key);
            await semaphore.Value.WaitAsync();

            return new Locker(this, semaphore, expiration);
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            var semaphore = GetOrCreateSemaphore(key);

            if (await semaphore.Value.WaitAsync(timeout != TimeSpan.MaxValue ? timeout : Timeout.InfiniteTimeSpan))
            {
                return (new Locker(this, semaphore, expiration), true);
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
            lock (_semaphores)
            {
                if (_semaphores.TryGetValue(key, out var semaphore))
                {
                    return Task.FromResult(semaphore.Value.CurrentCount == 0);
                }

                return Task.FromResult(false);
            }
        }

        private Semaphore GetOrCreateSemaphore(string key)
        {
            lock (_semaphores)
            {
                if (_semaphores.TryGetValue(key, out var semaphore))
                {
                    semaphore.RefCount++;
                }
                else
                {
                    semaphore = new Semaphore(key, new SemaphoreSlim(1));
                    _semaphores[key] = semaphore;
                }

                return semaphore;
            }
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
            private readonly LocalLock _localLock;
            private readonly Semaphore _semaphore;
            private readonly CancellationTokenSource _cts;
            private volatile int _released;
            private bool _disposed;

            public Locker(LocalLock localLock, Semaphore semaphore, TimeSpan? expiration)
            {
                _localLock = localLock;
                _semaphore = semaphore;

                if (expiration.HasValue && expiration.Value != TimeSpan.MaxValue)
                {
                    _cts = new CancellationTokenSource(expiration.Value);
                    _cts.Token.Register(Release);
                }
            }

            private void Release()
            {
                if (Interlocked.Exchange(ref _released, 1) == 0)
                {
                    lock (_localLock._semaphores)
                    {
                        if (_localLock._semaphores.TryGetValue(_semaphore.Key, out var semaphore))
                        {
                            semaphore.RefCount--;

                            if (semaphore.RefCount == 0)
                            {
                                _localLock._semaphores.Remove(_semaphore.Key);
                            }
                        }
                    }

                    _semaphore.Value.Release();
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
                semaphore.Value.Dispose();
            }
        }
    }
}
