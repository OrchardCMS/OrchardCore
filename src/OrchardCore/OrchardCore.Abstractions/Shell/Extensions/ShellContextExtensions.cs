using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell
{
    public static class ShellContextExtensions
    {
        /// <summary>
        /// Wether or not the tenant is only pre-created after the first loading.
        /// </summary>
        public static bool IsPreCreated(this ShellContext context) => context is ShellContext.PlaceHolder { PreCreated: true };

        /// <summary>
        /// Wether or not the tenant container is built after a first demand.
        /// </summary>
        public static bool IsBuilt(this ShellContext context) => context is { ServiceProvider: not null };

        /// <summary>
        /// Wether or not the tenant pipeline is built after a first request.
        /// </summary>
        public static bool IsWarmedUp(this ShellContext context) => context is { Pipeline: not null };

        /// <summary>
        /// Wether or not the tenant is in use in at least one active scope.
        /// </summary>
        public static bool IsInUse(this ShellContext context) => context is { ActiveScopes: > 0 };
    }
}
