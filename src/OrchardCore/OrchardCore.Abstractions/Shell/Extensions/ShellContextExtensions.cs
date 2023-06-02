using System;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell
{
    public static class ShellContextExtensions
    {
        /// <summary>
        /// Wether or not the tenant is only a placeholder built on loading, releasing or reloading.
        /// On first loading the <see cref="ShellContext.PlaceHolder.PreCreated"/> is equal to true.
        /// </summary>
        public static bool IsPlaceholder(this ShellContext context, Func<ShellContext.PlaceHolder, bool> filter = null)
        {
            if (context is not ShellContext.PlaceHolder placeholder)
            {
                return false;
            }

            if (filter is null)
            {
                return true;
            }

            return filter(placeholder);
        }

        /// <summary>
        /// Wether or not the tenant container has been built on a first demand.
        /// </summary>
        public static bool IsBuilt(this ShellContext context) => context is { ServiceProvider: not null };

        /// <summary>
        /// Wether or not the tenant pipeline has been built on a first request.
        /// </summary>
        public static bool IsWarmedUp(this ShellContext context) => context is { Pipeline: not null };

        /// <summary>
        /// Wether or not the tenant is in use in at least one active scope.
        /// </summary>
        public static bool IsInUse(this ShellContext context) => context is { ActiveScopes: > 0 };
    }
}
