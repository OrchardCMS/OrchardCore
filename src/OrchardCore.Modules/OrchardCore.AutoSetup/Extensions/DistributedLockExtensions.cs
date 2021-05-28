using System;
using System.Threading.Tasks;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.AutoSetup.Extensions
{
    /// <summary>
    /// The distributed lock extensions for auto setup.
    /// </summary>
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire an auto setup lock.
        /// </summary>
        /// <param name="distributedLock">
        /// The distributed lock.
        /// </param>
        /// <param name="lockOptions">
        /// The auto setup lock options.
        /// </param>
        /// <returns>
        /// The <see cref="ILocker"/> and <c>true</c> if successfully acquired.
        /// </returns>
        public static Task<(ILocker locker, bool locked)> TryAcquireAutoSetupLockAsync(this IDistributedLock distributedLock, LockOptions lockOptions)
        {
            var timeout = lockOptions?.LockTimeout ?? 0;
            if (timeout <= 0)
            {
                timeout = 30_000;
            }

            var expiration = lockOptions?.LockExpiration ?? 0;
            if (expiration <= 0)
            {
                expiration = 30_000;
            }

            return distributedLock.TryAcquireLockAsync(
                "AUTOSETUP_LOCK",
                TimeSpan.FromMilliseconds(timeout),
                TimeSpan.FromMilliseconds(expiration));
        }
    }
}
