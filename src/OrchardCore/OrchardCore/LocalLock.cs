using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore
{
    /// <summary>
    /// This component is a tenant singleton which allows to acquire named locks for a given tenant.
    /// This is a non distributed version where expiration times are not used to auto release locks.
    /// </summary>
    public class LocalLock : ILock, IDisposable
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// This is a non distributed version where the expiration time is not used to auto release the lock.
        /// </summary>
        public async Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan? timeout = null, TimeSpan? expiration = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));
            return (new Locker(semaphore), await semaphore.WaitAsync(timeout ?? TimeSpan.FromMilliseconds(-1)));
        }

        public async Task<IDisposable> AcquireLockAsync(string key)
        {
            var semaphore = _semaphores.GetOrAdd(key, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            return new Locker(semaphore);
        }

        private class Locker : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public Locker(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
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