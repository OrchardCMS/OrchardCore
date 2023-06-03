using System;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell
{
    public static class ShellContextExtensions
    {
        /// <summary>
        /// Wether or not the tenant is only a placeholder built on loading, releasing or reloading.
        /// On the first loading <see cref="ShellContext.PlaceHolder.PreCreated"/> is equal to true.
        /// </summary>
        public static bool IsPlaceholder(this ShellContext context, Func<ShellContext.PlaceHolder, bool> predicate = null)
            => context is ShellContext.PlaceHolder placeholder && (predicate?.Invoke(placeholder) ?? true);

        /// <summary>
        /// Wether or not the tenant pipeline has been built on a first request.
        /// </summary>
        public static bool HasPipeline(this ShellContext context) => context is { Pipeline: not null };

        /// <summary>
        /// Wether or not the tenant is in use in at least one active scope.
        /// </summary>
        public static bool IsActive(this ShellContext context) => context is { ActiveScopes: > 0 };
    }
}
