using System;
using System.Threading.Tasks;

namespace OrchardCore.Distributed.Locking
{
    public interface IDistributedLock
    {
        Task<bool> LockTakeAsync(string key, TimeSpan expiry);
        Task<bool> LockReleaseAsync(string key);
    }
}
