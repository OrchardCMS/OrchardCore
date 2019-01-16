using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface ITenantsSettingsSources
    {
        void AddSources(IConfigurationBuilder builder);
        void Save(string tenant, IDictionary<string, string> data);
    }

    public static class TenantsSettingsSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, ITenantsSettingsSources sources)
        {
            sources.AddSources(builder);
            return builder;
        }
    }
}
