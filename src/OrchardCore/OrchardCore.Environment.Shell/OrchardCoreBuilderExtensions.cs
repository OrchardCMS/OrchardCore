using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host level shell services.
        /// </summary>
        public static OrchardCoreBuilder AddShellHost(this OrchardCoreBuilder builder)
        {
            builder.Services.AddHostingShellServices();
            return builder;
        }
    }
}
