using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.BackgroundTasks
{
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a lock on the background task if it is atomic and if the lock service is not a local lock, otherwise returns true with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireBackgroundTaskLockAsync(this IDistributedLock distributedLock, BackgroundTaskSettings settings)
        {
            if (distributedLock is ILocalLock || !settings.IsAtomic)
            {
                return Task.FromResult<(ILocker, bool)>((null, true));
            }

            return distributedLock.TryAcquireLockAsync(
                settings.Name + "_LOCK",
                TimeSpan.FromMilliseconds(settings.LockTimeout),
                TimeSpan.FromMilliseconds(settings.LockExpiration));
        }
    }
}
