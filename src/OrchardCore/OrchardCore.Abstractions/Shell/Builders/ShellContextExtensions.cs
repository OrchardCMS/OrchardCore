using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Models;
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
            var lockService = shellContext.Settings.State == TenantState.Initializing
                ? (ILock)shellContext.ServiceProvider.GetRequiredService<ILocalLock>()
                : shellContext.ServiceProvider.GetRequiredService<IDistributedLock>();

            // If it is a local lock, use a maximum timeout and no expiration.
            if (lockService is ILocalLock localLock)
            {
                return localLock.TryAcquireLockAsync("SHELL_ACTIVATE_LOCK", TimeSpan.MaxValue);
            }

            // If it is a distributed lock, we use the configured locking times.
            var options = shellContext.ServiceProvider.GetRequiredService<IOptions<ShellContextOptions>>().Value;

            return lockService.TryAcquireLockAsync(
                "SHELL_ACTIVATE_LOCK",
                TimeSpan.FromMilliseconds(options.ShellActivateLockTimeout),
                TimeSpan.FromMilliseconds(options.ShellActivateLockExpiration));
        }
    }
}
