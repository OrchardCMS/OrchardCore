using OrchardCore.Dashboards;
using OrchardCore.Localization;

namespace Microsoft.Extensions.DependencyInjection;

public static class DashboardTypeServiceCollectionExtensions
{
    /// <summary>
    /// Registers a type of dashboard, a page made of widgets served at <c>/Admin/dashboards/{name}</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The technical name of the type, stored on its widgets and used in its URLs.</param>
    /// <param name="displayName">The name shown to users, as the title of the dashboard.</param>
    /// <param name="configure">An optional delegate to configure the type, for example its scope and permissions.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDashboardType(
        this IServiceCollection services,
        string name,
        LocalizationSource displayName,
        Action<DashboardTypeDescriptor> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(displayName);

        services.Configure<DashboardOptions>(options => options.Add(name, displayName, configure));

        return services;
    }
}
