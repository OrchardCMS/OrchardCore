using Microsoft.Extensions.Logging;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Locking;

/// <summary>
/// This component is a tenant singleton which allows to acquire named locks for a given tenant.
/// </summary>
public sealed class LocalLock : IDistributedLock, ILocalLock, IDisposable
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, NamedSemaphore> _semaphores = [];
    private bool _disposed;

    public LocalLock(ILogger<LocalLock> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
    /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
    /// </summary>
    public async Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var semaphore = GetOrCreateSemaphore(key);
        await semaphore.Value.WaitAsync();

        return new Locker(this, semaphore, expiration);
    }

    /// <summary>
    /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
    /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
    /// </summary>
    public async Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var semaphore = GetOrCreateSemaphore(key);

        var effectiveTimeout = timeout != TimeSpan.MaxValue ? timeout : Timeout.InfiniteTimeSpan;
        if (await semaphore.Value.WaitAsync(effectiveTimeout))
        {
            return (new Locker(this, semaphore, expiration), true);
        }

        // Decrement refcount on timeout (we incremented when getting/creating the semaphore).
        var removed = ReleaseReference(semaphore);

        if (removed)
        {
            // No one else references this semaphore, dispose it.
            semaphore.Value.Dispose();
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Timeout elapsed before acquiring the named lock '{LockName}' after the given timeout of '{Timeout}'.", key, timeout);
        }

        return (null, false);
    }

    public Task<bool> IsLockAcquiredAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(key, out var semaphore))
            {
                return Task.FromResult(semaphore.Value.CurrentCount == 0);
            }

            return Task.FromResult(false);
        }
    }

    private NamedSemaphore GetOrCreateSemaphore(string key)
    {
        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(key, out var semaphore))
            {
                semaphore.RefCount++;
            }
            else
            {
                semaphore = new NamedSemaphore(key, new SemaphoreSlim(1));
                _semaphores[key] = semaphore;
            }

            return semaphore;
        }
    }

    // Returns true if the semaphore was removed (RefCount reached 0) and should be disposed by the caller.
    private bool ReleaseReference(NamedSemaphore semaphoreRef)
    {
        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(semaphoreRef.Key, out var semaphore))
            {
                semaphore.RefCount--;

                if (semaphore.RefCount == 0)
                {
                    _semaphores.Remove(semaphoreRef.Key);
                    return true;
                }
            }

            return false;
        }
    }

    private sealed class NamedSemaphore
    {
        public NamedSemaphore(string key, SemaphoreSlim value)
        {
            Key = key;
            Value = value;
            RefCount = 1;
        }

        internal string Key { get; }
        internal SemaphoreSlim Value { get; }
        internal int RefCount { get; set; }
    }

    private sealed class Locker : ILocker
    {
        private readonly LocalLock _localLock;
        private readonly NamedSemaphore _semaphore;
        private readonly CancellationTokenSource _cts;
        private volatile int _released;
        private bool _disposed;

        public Locker(LocalLock localLock, NamedSemaphore semaphore, TimeSpan? expiration)
        {
            _localLock = localLock;
            _semaphore = semaphore;

            if (expiration.HasValue && expiration.Value != TimeSpan.MaxValue)
            {
                _cts = new CancellationTokenSource(expiration.Value);
                _cts.Token.Register(Release);
            }
        }

        private void Release()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                // Decrement refcount and potentially remove from dictionary first.
                var removed = _localLock.ReleaseReference(_semaphore);

                // Then release the semaphore to allow next waiter (if any) to proceed.
                _semaphore.Value.Release();

                // If no one else references it anymore, dispose it.
                if (removed)
                {
                    _semaphore.Value.Dispose();
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _cts?.Dispose();

            Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Dispose only semaphores that are not in use to avoid race issues with active lockers.
        List<NamedSemaphore> toDispose;
        lock (_semaphores)
        {
            toDispose = _semaphores.Values.Where(s => s.RefCount == 0).ToList();
            // Remove entries we are disposing.
            foreach (var s in toDispose)
            {
                _semaphores.Remove(s.Key);
            }
        }

        foreach (var semaphore in toDispose)
        {
            semaphore.Value.Dispose();
        }
    }
}
