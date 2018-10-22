using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Environment.Shell
{
    public static class ShellHostExtensions
    {
        /// <summary>
        /// Retrieves the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns>The shell settings associated with the tenant.</returns>
        public static ShellSettings GetSettings(this IShellHost shellHost, string name)
        {
            if (!shellHost.TryGetSettings(name, out ShellSettings settings))
            {
                throw new ArgumentException("The specified tenant name is not valid.", nameof(name));
            }

            return settings;
        }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="tenant">The tenant name related to the service scope to get.</param>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        public static async Task<IServiceScope> GetScopeAsync(this IShellHost shellHost, string tenant)
        {
            return (await shellHost.GetScopeAndContextAsync(shellHost.GetSettings(tenant))).Scope;
        }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="tenant">The tenant name related to the scope and the shell to get.</param>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        public static Task<(IServiceScope Scope, ShellContext ShellContext)> GetScopeAndContextAsync(this IShellHost shellHost, string tenant)
        {
            return shellHost.GetScopeAndContextAsync(shellHost.GetSettings(tenant));
        }
    }
}
