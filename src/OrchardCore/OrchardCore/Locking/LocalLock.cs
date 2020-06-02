using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Locking
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire named locks for a given tenant.
    /// This is a non distributed version where expiration times are not used to auto release locks.
    /// </summary>
    public class LocalLock : IDistributedLock, IDisposable
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// This is a non distributed version where the expiration time is not used to auto release the lock.
        /// </summary>
        public async Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            return new Locker(semaphore);
        }

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// This is a non distributed version where the expiration time is not used to auto release the lock.
        /// </summary>
        public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));

            if (await semaphore.WaitAsync(timeout))
            {
                return (new Locker(semaphore), true);
            }

            return (null, false);
        }

        private class Locker : ILocker
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed;

            public Locker(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _semaphore.Release();
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
