using System;
using System.Threading.Tasks;

namespace OrchardCore.Distributed
{
    public interface IDistributedLock
    {
        Task<bool> LockTakeAsync(string key, TimeSpan expiry);
        Task<bool> LockReleaseAsync(string key);
    }
}
