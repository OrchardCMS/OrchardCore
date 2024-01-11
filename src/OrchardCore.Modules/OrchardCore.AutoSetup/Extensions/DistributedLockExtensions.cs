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
            TimeSpan timeout, expiration;
            if (distributedLock is ILocalLock)
            {
                // If it is a local lock, don't use any timeout and expiration.
                timeout = expiration = TimeSpan.MaxValue;
            }
            else
            {
                // If it is a distributed lock, use the configured timeout and expiration.
                var lockTimeout = lockOptions?.LockTimeout ?? 0;
                if (lockTimeout <= 0)
                {
                    lockTimeout = 60_000;
                }

                var lockExpiration = lockOptions?.LockExpiration ?? 0;
                if (lockExpiration <= 0)
                {
                    lockExpiration = 60_000;
                }

                timeout = TimeSpan.FromMilliseconds(lockTimeout);
                expiration = TimeSpan.FromMilliseconds(lockExpiration);
            }

            return distributedLock.TryAcquireLockAsync("AUTOSETUP_LOCK", timeout, expiration);
        }
    }
}
