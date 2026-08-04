using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using tusdotnet.Interfaces;

namespace OrchardCore.Media.Services;

/// <summary>
/// TUS file lock provider backed by <see cref="IDistributedLock"/>.
/// In single-server deployments this uses <see cref="ILocalLock"/>;
/// with Redis enabled it uses the distributed lock.
/// </summary>
internal sealed class DistributedFileLockProvider : ITusFileLockProvider
{
    private readonly ILock _lock;

    public DistributedFileLockProvider(IDistributedLock distributedLock)
    {
        _lock = distributedLock;
    }

    public async Task<ITusFileLock> AquireLock(string fileId)
    {
        var key = $"tus:lock:{fileId}";

        // Try to acquire with a short timeout — TUS clients will retry on 423.
        var (locker, locked) = await _lock.TryAcquireLockAsync(key, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));

        return new DistributedTusFileLock(locker, locked);
    }

    private sealed class DistributedTusFileLock : ITusFileLock
    {
        private readonly ILocker _locker;
        private readonly bool _locked;

        public DistributedTusFileLock(ILocker locker, bool locked)
        {
            _locker = locker;
            _locked = locked;
        }

        public Task<bool> Lock()
        {
            return Task.FromResult(_locked);
        }

        public async Task ReleaseIfHeld()
        {
            if (_locker != null)
            {
                await _locker.DisposeAsync();
            }
        }
    }
}
