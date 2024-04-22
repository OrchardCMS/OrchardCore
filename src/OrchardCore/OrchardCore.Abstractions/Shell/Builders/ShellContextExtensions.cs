using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Environment.Shell.Builders
{
    public static class ShellContextExtensions
    {
        /// <summary>
        /// Tries to acquire a lock for shell activation, a local lock if it is initializing, otherwise a distributed lock.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireShellActivateLockAsync(this ShellContext shellContext)
        {
            // If the shell is initializing, force the usage of a local lock.
            var lockService = shellContext.Settings.IsInitializing()
                ? (ILock)shellContext.ServiceProvider.GetRequiredService<ILocalLock>()
                : shellContext.ServiceProvider.GetRequiredService<IDistributedLock>();

            TimeSpan timeout, expiration;
            if (lockService is ILocalLock)
            {
                // If it is a local lock, don't use any timeout and expiration.
                timeout = expiration = TimeSpan.MaxValue;
            }
            else
            {
                // If it is a distributed lock, use the configured timeout and expiration.
                var options = shellContext.ServiceProvider.GetRequiredService<IOptions<ShellContextOptions>>();
                timeout = TimeSpan.FromMilliseconds(options.Value.ShellActivateLockTimeout);
                expiration = TimeSpan.FromMilliseconds(options.Value.ShellActivateLockExpiration);
            }

            return lockService.TryAcquireLockAsync("SHELL_ACTIVATE_LOCK", timeout, expiration);
        }

        /// <summary>
        /// Tries to acquire a lock for shell removing.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireShellRemovingLockAsync(this ShellContext shellContext)
        {
            TimeSpan timeout, expiration;

            var lockService = shellContext.ServiceProvider.GetRequiredService<IDistributedLock>();
            if (lockService is ILocalLock)
            {
                // If it is a local lock, don't use any timeout and expiration.
                timeout = expiration = TimeSpan.MaxValue;
            }
            else
            {
                // If it is a distributed lock, use the configured timeout and expiration.
                var options = shellContext.ServiceProvider.GetRequiredService<IOptions<ShellContextOptions>>();
                timeout = TimeSpan.FromMilliseconds(options.Value.ShellRemovingLockTimeout);
                expiration = TimeSpan.FromMilliseconds(options.Value.ShellRemovingLockExpiration);
            }

            return lockService.TryAcquireLockAsync("SHELL_REMOVING_LOCK", timeout, expiration);
        }

        /// <summary>
        /// Creates a <see cref="ShellScope"/> on this shell context.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use CreateScopeAsync instead.", false)]
        public static ShellScope CreateScope(this ShellContext shellContext) =>
            shellContext.CreateScopeAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Mark the <see cref="ShellContext"/> as released and then a candidate to be disposed.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use ReleaseAsync instead.", false)]
        public static void Release(this ShellContext shellContext) =>
            shellContext.ReleaseInternalAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Registers the specified shellContext as dependent such that it is also released when the current shell context is released.
        /// </summary>
        [Obsolete("This method will be removed in a future version, use AddDependentShellAsync instead.", false)]
        public static void AddDependentShell(this ShellContext shellContext) =>
            shellContext.AddDependentShellAsync(shellContext).GetAwaiter().GetResult();
    }
}
