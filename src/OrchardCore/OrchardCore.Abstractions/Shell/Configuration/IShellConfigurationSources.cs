using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellConfigurationSources
    {
        void AddSources(IConfigurationBuilder builder);
    }

    public static class ShellConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IShellConfigurationSources sources)
        {
            sources?.AddSources(builder);
            return builder;
        }
    }
}
