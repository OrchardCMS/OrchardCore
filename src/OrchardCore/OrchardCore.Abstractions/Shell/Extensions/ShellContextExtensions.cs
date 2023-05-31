using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell
{
    public static class ShellContextExtensions
    {
        /// <summary>
        /// Wether or not the tenant is pre-built but only after the first loading.
        /// </summary>
        public static bool IsPreCreated(this ShellContext context) =>
            context is ShellContext.PlaceHolder placeHolder && placeHolder.PreCreated;

        /// <summary>
        /// Wether or not the tenant is pre-built after a releasing or reloading.
        /// </summary>
        public static bool IsPreBuilt(this ShellContext context) =>
            context is ShellContext.PlaceHolder placeHolder && !placeHolder.PreCreated;

        /// <summary>
        /// Wether or not the tenant container is built after a first demand.
        /// </summary>
        public static bool IsBuilt(this ShellContext context) => context?.ServiceProvider is not null;

        /// <summary>
        /// Wether or not the tenant pipeline is built after a first request.
        /// </summary>
        public static bool HasPipeline(this ShellContext context) => context?.Pipeline is not null;

        /// <summary>
        /// Wether or not the tenant is in use in at least one active scope.
        /// </summary>
        public static bool IsInUse(this ShellContext context) => context is not null && context.ActiveScopes != 0;
    }
}
