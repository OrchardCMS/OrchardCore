using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Clusters;

/// <summary>
/// Adds host and tenant level services for managing tenant clusters.
/// </summary>
public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Add tenant level services to auto release clustered tenants on inactivity.
    /// </summary>
    public static OrchardCoreBuilder AddClusteredTenantInactivityCheck(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IBackgroundTask, ClusteredTenantInactivityCheck>();
        });

        return builder;
    }
}
