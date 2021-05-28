using System;
using System.Threading.Tasks;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.AutoSetup.Extensions
{
    /// <summary>
    /// The distributed lock extensions for Auto Setup.
    /// </summary>
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a setup lock 
        /// </summary>
        /// <param name="distributedLock">
        /// The distributed Lock.
        /// </param>
        /// <param name="lockOptions">
        /// The setup options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<(ILocker locker, bool locked)> TryAcquireAutoSetupLockAsync(this IDistributedLock distributedLock, LockOptions lockOptions)
        {
            if (lockOptions == null)
            {
                throw new ArgumentNullException(nameof(lockOptions));
            }

            return distributedLock.TryAcquireLockAsync(
                "AUTOSETUP_LOCK",
                TimeSpan.FromMilliseconds(lockOptions.LockTimeout),
                TimeSpan.FromMilliseconds(lockOptions.LockExpiration));
        }
    }
}
