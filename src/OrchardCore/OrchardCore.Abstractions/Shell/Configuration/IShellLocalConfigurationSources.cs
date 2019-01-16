using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellLocalConfigurationSources
    {
        void AddSources(string tenant, IConfigurationBuilder builder);
        void Save(string tenant, IDictionary<string, string> data);
    }

    public static class ShellLocalConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, string tenant, IShellLocalConfigurationSources sources)
        {
            sources?.AddSources(tenant, builder);
            return builder;
        }
    }
}
