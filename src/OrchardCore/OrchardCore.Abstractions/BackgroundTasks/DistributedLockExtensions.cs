using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.BackgroundTasks
{
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a distributed lock on the related background task. If the lock service is a local implementation returns true but with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireBackgroundTaskLockAsync(this IDistributedLock distributedLock, BackgroundTaskSettings settings)
        {
            if (distributedLock is ILocalLock || settings.LockTimeout <= 0 || settings.LockExpiration <= 0)
            {
                return Task.FromResult<(ILocker, bool)>((null, true));
            }

            return distributedLock.TryAcquireLockAsync(
                "BGT_" + settings.Name + "_LOCK",
                TimeSpan.FromMilliseconds(settings.LockTimeout),
                TimeSpan.FromMilliseconds(settings.LockExpiration));
        }
    }
}
