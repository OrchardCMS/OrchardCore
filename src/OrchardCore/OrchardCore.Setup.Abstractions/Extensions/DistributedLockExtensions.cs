using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Setup.Services.Extensions
{
    /// <summary>
    /// The distributed lock extensions for Setup.
    /// </summary>
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a setup lock 
        /// </summary>
        /// <param name="distributedLock">
        /// The distributed Lock.
        /// </param>
        /// <param name="options">
        /// The setup options.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task<(ILocker locker, bool locked)> TryAcquireSetupLockAsync(this IDistributedLock distributedLock, SetupOptions options)
        {
            return distributedLock.TryAcquireLockAsync(
                options.SetupLockName,
                TimeSpan.FromMilliseconds(options.SetupLockTimeout),
                TimeSpan.FromMilliseconds(options.SetupLockExpiration));
        }
    }
}
