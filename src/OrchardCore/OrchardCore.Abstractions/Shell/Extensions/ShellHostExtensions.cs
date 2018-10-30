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

        /// <summary>
        /// Tries to creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="tenant">The tenant name related to the service scope to get.</param>
        /// <returns>An associated scope if the tenant name is valid, otherwise null.</returns>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        public static async Task<IServiceScope> TryGetScopeAsync(this IShellHost shellHost, string tenant)
        {
            if (!shellHost.TryGetSettings(tenant, out var settings))
            {
                return null;
            }

            return (await shellHost.GetScopeAndContextAsync(settings)).Scope;
        }


        /// <summary>
        /// Tries to creates a standalone service scope that can be used to resolve local services and
        /// replaces <see cref="HttpContext.RequestServices"/> with it.
        /// </summary>
        /// <param name="tenant">The tenant name related to the scope and the shell to get.</param>
        /// <returns>Associated scope and shell if the tenant name is valid, otherwise null values.</returns>
        /// <remarks>
        /// Disposing the returned <see cref="IServiceScope"/> instance restores the previous state.
        /// </remarks>
        public static Task<(IServiceScope Scope, ShellContext ShellContext)> TryGetScopeAndContextAsync(this IShellHost shellHost, string tenant)
        {
            if (!shellHost.TryGetSettings(tenant, out var settings))
            {
                IServiceScope scope = null;
                ShellContext shell = null;
                return Task.FromResult((scope, shell));
            }

            return shellHost.GetScopeAndContextAsync(settings);
        }

        /// <summary>
        /// Allows to trigger a shell event through a delegate having an 'IShellEvents' parameter.
        /// </summary>
        public static async Task ShellEventAsync(this IShellHost shellHost, Func<IShellEvents, Task> handler)
        {
            if (shellHost.TryGetSettings(ShellHelper.DefaultShellName, out var settings))
            {
                using (var scope = await shellHost.GetScopeAsync(settings))
                {
                    var events = scope.ServiceProvider.GetService<IShellEvents>();

                    if (events != null)
                    {
                        await handler(events);
                    }
                }
            }
        }
    }
}
