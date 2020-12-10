using System;
using System.Threading.Tasks;

namespace OrchardCore.Locking
{
    public interface ILock
    {
        /// <summary>
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null);

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null);

        /// <summary>
        /// Whether a named lock is already acquired.
        /// </summary>
        Task<bool> IsLockAcquiredAsync(string key);
    }
}
