using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders;
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
        public static Task<(ILocker locker, bool locked)> TryAcquireActivateShellLockAsync(this ShellContext shellContext)
        {
            var lockService = shellContext.Settings.State == TenantState.Initializing
                ? (ILock)shellContext.ServiceProvider.GetRequiredService<ILocalLock>()
                : shellContext.ServiceProvider.GetRequiredService<IDistributedLock>();

            return lockService.TryAcquireLockAsync(
                "ACTIVATE_SHELL_LOCK",
                TimeSpan.FromMilliseconds(10_000),
                TimeSpan.FromMilliseconds(10_000));
        }
    }
}
