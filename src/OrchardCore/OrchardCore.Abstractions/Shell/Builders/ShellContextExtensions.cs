using System;
using System.Threading;
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
        /// Tries to acquire a lock on this shell, a distributed lock if it is not initializing, otherwise a local lock.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireShellActivateLockAsync(this ShellContext shellContext)
        {
            var lockService = shellContext.Settings.State == TenantState.Initializing
                ? (ILock)shellContext.ServiceProvider.GetRequiredService<ILocalLock>()
                : shellContext.ServiceProvider.GetRequiredService<IDistributedLock>();

            if (lockService is ILocalLock localLock)
            {
                return localLock.TryAcquireLockAsync("SHELL_ACTIVATE_LOCK", timeout: TimeSpan.MaxValue);
            }

            var options = shellContext.ServiceProvider.GetRequiredService<IOptions<ShellContextOptions>>().Value;

            return lockService.TryAcquireLockAsync(
                "SHELL_ACTIVATE_LOCK",
                TimeSpan.FromMilliseconds(options.ShellActivateLockTimeout),
                TimeSpan.FromMilliseconds(options.ShellActivateLockExpiration));
        }
    }
}
