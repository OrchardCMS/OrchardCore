using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellConfigurationSources
    {
        void AddSources(string tenant, IConfigurationBuilder builder);
        Task SaveAsync(string tenant, IDictionary<string, string> data);
    }

    public static class ShellConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, string tenant, IShellConfigurationSources sources)
        {
            sources.AddSources(tenant, builder);
            return builder;
        }
    }
}
