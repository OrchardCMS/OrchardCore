using System;
using System.Threading.Tasks;

namespace OrchardCore
{
    public interface ILock
    {
        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// Note: A non distributed implementation doesn't use the expiration time to auto release the lock.
        /// </summary>
        Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan? timeout = null, TimeSpan? expiration = null);
    }
}
