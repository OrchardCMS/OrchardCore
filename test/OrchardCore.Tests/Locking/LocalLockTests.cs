using System.Diagnostics;
using OrchardCore.Locking;

namespace OrchardCore.Tests.Locking;

public class LocalLockTests
{
    private static LocalLock CreateLockService() => new LocalLock(NullLogger<LocalLock>.Instance);

    [Fact]
    public async Task Acquire_Then_TryAcquire_SameKey_EnforcesExclusivity()
    {
        using var localLock = CreateLockService();

        // Acquire the lock for a given key.
        using var locker1 = await localLock.AcquireLockAsync("KEY1");
        Assert.True(await localLock.IsLockAcquiredAsync("KEY1"));

        // Try to acquire the same lock with zero timeout, should fail.
        var (locker2, locked2) = await localLock.TryAcquireLockAsync("KEY1", TimeSpan.Zero);
        Assert.False(locked2);
        Assert.Null(locker2);

        // Release the first lock, then acquire again should succeed.
        locker1.Dispose();

        var (locker3, locked3) = await localLock.TryAcquireLockAsync("KEY1", TimeSpan.FromMilliseconds(200));
        Assert.True(locked3);
        Assert.NotNull(locker3);
        locker3.Dispose();

        Assert.False(await localLock.IsLockAcquiredAsync("KEY1"));
    }

    [Fact]
    public async Task TryAcquire_With_Timeout_Fails_When_Lock_Is_Held()
    {
        using var localLock = CreateLockService();

        using var _ = await localLock.AcquireLockAsync("TIMEOUT_KEY");

        // Should time out and not acquire the lock.
        var sw = Stopwatch.StartNew();
        var (locker, locked) = await localLock.TryAcquireLockAsync("TIMEOUT_KEY", TimeSpan.FromMilliseconds(50));
        sw.Stop();

        Assert.False(locked);
        Assert.Null(locker);
        Assert.True(sw.ElapsedMilliseconds >= 45, $"Expected ~50ms timeout, got {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Acquire_With_Expiration_Auto_Releases()
    {
        using var localLock = CreateLockService();

        // Acquire with short expiration; it should auto-release after it elapses.
        var locker = await localLock.AcquireLockAsync("EXP_KEY", TimeSpan.FromMilliseconds(100));
        Assert.True(await localLock.IsLockAcquiredAsync("EXP_KEY"));

        // Wait long enough for expiration callback to fire and release the semaphore.
        await Task.Delay(300, TestContext.Current.CancellationToken);

        Assert.False(await localLock.IsLockAcquiredAsync("EXP_KEY"));

        var (locker2, locked2) = await localLock.TryAcquireLockAsync("EXP_KEY", TimeSpan.FromMilliseconds(50));
        Assert.True(locked2);
        locker2.Dispose();

        // Dispose the original locker after it auto-released (idempotent release).
        await locker.DisposeAsync();
    }

    [Fact]
    public async Task TryAcquire_With_InfiniteTimeout_Succeeds_When_Free()
    {
        using var localLock = CreateLockService();

        var (locker, locked) = await localLock.TryAcquireLockAsync("FREE_KEY", TimeSpan.MaxValue);
        Assert.True(locked);
        Assert.NotNull(locker);
        locker.Dispose();
    }

    [Fact]
    public async Task Different_Keys_Are_Independent()
    {
        using var localLock = CreateLockService();

        using var locker1 = await localLock.AcquireLockAsync("A");
        var (locker2, locked2) = await localLock.TryAcquireLockAsync("B", TimeSpan.FromMilliseconds(10));

        Assert.True(locked2);
        Assert.NotNull(locker2);
        locker2.Dispose();
    }

    [Fact]
    public async Task IsLockAcquired_ReturnsFalse_When_NotHeld()
    {
        using var localLock = CreateLockService();

        Assert.False(await localLock.IsLockAcquiredAsync("UNKNOWN_KEY"));

        using var locker = await localLock.AcquireLockAsync("UNKNOWN_KEY");
        Assert.True(await localLock.IsLockAcquiredAsync("UNKNOWN_KEY"));

        locker.Dispose();
        Assert.False(await localLock.IsLockAcquiredAsync("UNKNOWN_KEY"));
    }

    [Fact]
    public async Task Dispose_LocalLock_Throws_On_Public_Members()
    {
        var localLock = CreateLockService();

        // Sanity: works before Dispose
        using var locker = await localLock.AcquireLockAsync("DISPOSE_KEY");

        localLock.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => localLock.AcquireLockAsync("DISPOSE_KEY"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => localLock.TryAcquireLockAsync("DISPOSE_KEY", TimeSpan.Zero));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => localLock.IsLockAcquiredAsync("DISPOSE_KEY"));

        // Disposing locker should still be safe even if LocalLock is disposed.
        locker.Dispose();
    }
}
