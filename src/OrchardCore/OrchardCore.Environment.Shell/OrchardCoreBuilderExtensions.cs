using OrchardCore.Environment.Shell.Internal;
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
            ShellServiceCollection.AddShellHostServices(builder.Services);
            return builder;
        }
    }
}
