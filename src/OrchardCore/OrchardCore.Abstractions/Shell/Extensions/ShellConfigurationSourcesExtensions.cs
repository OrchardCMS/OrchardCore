using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public static class ShellConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IShellConfigurationSources sources)
        {
            sources?.AddSources(builder);
            return builder;
        }

        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, string tenant, IShellConfigurationSources sources)
        {
            sources?.AddSources(tenant, builder);
            return builder;
        }
    }
}