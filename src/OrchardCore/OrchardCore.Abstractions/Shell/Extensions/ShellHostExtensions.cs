using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    public static class ShellHostExtensions
    {
        /// <summary>
        /// Retrieves the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns>The shell settings associated with the tenant.</returns>
        public static ShellSettings GetSettings(this IShellHost shellHost, string tenant)
        {
            if (!shellHost.TryGetSettings(tenant, out var settings))
            {
                throw new ArgumentException("The specified tenant name is not valid.", nameof(tenant));
            }

            return settings;
        }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        /// <param name="tenant">The tenant name related to the service scope to get.</param>
        public static Task<ShellScope> GetScopeAsync(this IShellHost shellHost, string tenant)
        {
            return shellHost.GetScopeAsync(shellHost.GetSettings(tenant));
        }
    }
}
