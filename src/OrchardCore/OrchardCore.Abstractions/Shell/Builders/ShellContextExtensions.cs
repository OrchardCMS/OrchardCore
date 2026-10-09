using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Environment.Shell.Builders;

public static class ShellContextExtensions
{
    /// <summary>
    /// Tries to acquire a lock for shell activation, a local lock if it is initializing, otherwise a distributed lock.
    /// </summary>
    public static Task<(ILocker? locker, bool locked)> TryAcquireShellActivateLockAsync(this ShellContext shellContext)
    {
        var serviceProvider = shellContext.ServiceProvider ?? throw new InvalidOperationException("The shell service provider is not available.");

        // If the shell is initializing, force the usage of a local lock.
        var lockService = shellContext.Settings.IsInitializing()
            ? (ILock)serviceProvider.GetRequiredService<ILocalLock>()
            : serviceProvider.GetRequiredService<IDistributedLock>();

        TimeSpan timeout, expiration;
        if (lockService is ILocalLock)
        {
            // If it is a local lock, don't use any timeout and expiration.
            timeout = expiration = TimeSpan.MaxValue;
        }
        else
        {
            // If it is a distributed lock, use the configured timeout and expiration.
            var options = serviceProvider.GetRequiredService<IOptions<ShellContextOptions>>();
            timeout = TimeSpan.FromMilliseconds(options.Value.ShellActivateLockTimeout);
            expiration = TimeSpan.FromMilliseconds(options.Value.ShellActivateLockExpiration);
        }

        return lockService.TryAcquireLockAsync("SHELL_ACTIVATE_LOCK", timeout, expiration);
    }

    /// <summary>
    /// Tries to acquire a lock for shell removing.
    /// </summary>
    public static Task<(ILocker? locker, bool locked)> TryAcquireShellRemovingLockAsync(this ShellContext shellContext)
    {
        TimeSpan timeout, expiration;

        var serviceProvider = shellContext.ServiceProvider ?? throw new InvalidOperationException("The shell service provider is not available.");
        var lockService = serviceProvider.GetRequiredService<IDistributedLock>();
        if (lockService is ILocalLock)
        {
            // If it is a local lock, don't use any timeout and expiration.
            timeout = expiration = TimeSpan.MaxValue;
        }
        else
        {
            // If it is a distributed lock, use the configured timeout and expiration.
            var options = serviceProvider.GetRequiredService<IOptions<ShellContextOptions>>();
            timeout = TimeSpan.FromMilliseconds(options.Value.ShellRemovingLockTimeout);
            expiration = TimeSpan.FromMilliseconds(options.Value.ShellRemovingLockExpiration);
        }

        return lockService.TryAcquireLockAsync("SHELL_REMOVING_LOCK", timeout, expiration);
    }
}
