using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellsConfigurationSources
    {
        void AddSources(IConfigurationBuilder builder);
    }

    public static class ShellsConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IShellsConfigurationSources sources)
        {
            sources.AddSources(builder);
            return builder;
        }
    }
}
