using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Clusters;

/// <summary>
/// Adds host and tenant level services for managing tenant clusters.
/// </summary>
public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Registers a tenant level component to auto release tenants after the max idle time of their cluster.
    /// </summary>
    public static OrchardCoreBuilder AddTenantInactivityCheck(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IBackgroundTask, TenantInactivityBackgroundTask>();
        });

        return builder;
    }
}
