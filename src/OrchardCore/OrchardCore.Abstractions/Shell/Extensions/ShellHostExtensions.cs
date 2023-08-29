using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    public static class ShellHostExtensions
    {
        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        public static Task<ShellScope> GetScopeAsync(this IShellHost shellHost, string tenant)
        {
            return shellHost.GetScopeAsync(shellHost.GetSettings(tenant));
        }

        /// <summary>
        /// Reloads all shell settings and releases all shells so that new ones will be
        /// built for subsequent requests, while existing requests get flushed.
        /// </summary>
        public async static Task ReloadAllShellContextsAsync(this IShellHost shellHost)
        {
            foreach (var settings in shellHost.GetAllSettings())
            {
                await shellHost.ReloadShellContextAsync(settings);
            }
        }

        /// <summary>
        /// Releases all shells so that new ones will be built for subsequent requests.
        /// Note: Can be used to free up resources after a given period of inactivity.
        /// </summary>
        public async static Task ReleaseAllShellContextsAsync(this IShellHost shellHost)
        {
            foreach (var settings in shellHost.GetAllSettings())
            {
                await shellHost.ReleaseShellContextAsync(settings);
            }
        }

        /// <summary>
        /// Retrieves the shell settings associated with the specified tenant.
        /// </summary>
        public static ShellSettings GetSettings(this IShellHost shellHost, string tenant)
        {
            if (!shellHost.TryGetSettings(tenant, out var settings))
            {
                throw new ArgumentException("The specified tenant name is not valid.", nameof(tenant));
            }

            return settings;
        }

        /// <summary>
        /// Whether or not a given tenant is in use in at least one active scope.
        /// </summary>
        public static bool IsShellActive(this IShellHost shellHost, ShellSettings settings) =>
            settings is { Name: not null } &&
            shellHost.TryGetShellContext(settings.Name, out var context) &&
            context.IsActive();
    }
}
