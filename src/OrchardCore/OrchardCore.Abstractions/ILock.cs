using System;
using System.Threading.Tasks;

namespace OrchardCore
{
    public interface ILock
    {
        /// <summary>
        /// Tries to immediately acquire a named lock with a given expiration time within the current tenant.
        /// Note: A non distributed implementation doesn't use the expiration time to auto release the lock.
        /// </summary>
        Task<(IDisposable locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan? expiration = null);
    }
}
