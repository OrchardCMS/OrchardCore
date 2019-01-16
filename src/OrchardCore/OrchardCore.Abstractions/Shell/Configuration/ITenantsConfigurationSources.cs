using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public interface ITenantsConfigurationSources
    {
        void AddSources(IConfigurationBuilder builder);
    }

    public static class TenantsConfigurationSourcesExtensions
    {
        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, ITenantsConfigurationSources sources)
        {
            sources.AddSources(builder);
            return builder;
        }

        public static IConfigurationBuilder AddSources(this IConfigurationBuilder builder, IEnumerable<ITenantsConfigurationSources> sources)
        {
            foreach (var tenantsSources in sources)
            {
                tenantsSources.AddSources(builder);
            }

            return builder;
        }
    }
}
