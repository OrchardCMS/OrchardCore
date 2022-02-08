using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.BackgroundJobs
{
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a lock on the background task if it is atomic and if the lock service is not a local lock, otherwise returns true with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireBackgroundJobLockAsync(this IDistributedLock distributedLock, string backgroundJobId)
//=> distributedLock.TryAcquireLockAsync(
//    backgroundJobId + "_LOCK",
//    TimeSpan.FromMilliseconds(20_000),
//    TimeSpan.FromMilliseconds(20_000));
=> distributedLock.TryAcquireLockAsync(
                backgroundJobId + "_LOCK", TimeSpan.FromMilliseconds(20_000));
    }
}
