using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface IShellsSettingsSources
    {
        void AddSources(IConfigurationBuilder builder);
        Task SaveAsync(string tenant, IDictionary<string, string> data);
    }

    public static class ShellsSettingsSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IShellsSettingsSources sources)
        {
            sources.AddSources(builder);
            return builder;
        }
    }
}
