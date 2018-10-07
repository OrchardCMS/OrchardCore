using System;
using System.Threading.Tasks;

namespace OrchardCore.Distributed
{
    public interface IDistributedLock
    {
        Task<bool> LockAsync(string key, TimeSpan expiry);
        Task<bool> ReleaseAsync(string key);
    }
}
