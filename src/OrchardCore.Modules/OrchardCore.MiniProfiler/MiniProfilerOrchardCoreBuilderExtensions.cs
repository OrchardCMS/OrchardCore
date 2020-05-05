using OrchardCore.MiniProfiler;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MiniProfilerOrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Allow Mini Profiler on the admin too when the feature is enabled, not just on the frontend.
        /// </summary>
        public static OrchardCoreBuilder AllowMiniProfilerOnAdmin(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureServices(services =>
                services.Configure<MiniProfilerOptions>(settings => settings.AllowOnAdmin = true));
        }
    }
}
