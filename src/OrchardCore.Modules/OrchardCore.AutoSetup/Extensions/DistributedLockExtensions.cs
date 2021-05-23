using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.AutoSetup.Options;

namespace OrchardCore.AutoSetup.Extensions
{
    /// <summary>
    /// The distributed lock extensions for Auto Setup.
    /// </summary>
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Lock Prefix
        /// </summary>
        private const string LockPrefix = "AUTOSETUP_LOCK";

        /// <summary>
        /// The default timeout in milliseconds to acquire a distributed setup lock.
        /// </summary>
        private const int DefaultLockTimeout = 20_000;

        /// <summary>
        /// The default expiration in milliseconds of the distributed setup lock.
        /// </summary>
        private const int DefaultLockExpiration = 20_000;

        /// <summary>
        /// Tries to acquire a setup lock 
        /// </summary>
        /// <param name="distributedLock">
        /// The distributed Lock.
        /// </param>
        /// <param name="shellName">
        /// The shell Name for which lock acquires.
        /// </param>
        /// <param name="lockOptions">
        /// The setup options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<(ILocker locker, bool locked)> TryAcquireAutoSetupLockAsync(this IDistributedLock distributedLock, string shellName, LockOptions lockOptions)
        {
            if (lockOptions == null)
            {
                throw new ArgumentNullException(nameof(lockOptions));
            }

            return distributedLock.TryAcquireLockAsync(
                $"{LockPrefix}_{lockOptions.LockName ?? shellName}",
                TimeSpan.FromMilliseconds(lockOptions.LockTimeout == 0 ? DefaultLockTimeout : lockOptions.LockTimeout),
                TimeSpan.FromMilliseconds(lockOptions.LockExpiration == 0 ? DefaultLockExpiration : lockOptions.LockExpiration));
        }
    }
}
