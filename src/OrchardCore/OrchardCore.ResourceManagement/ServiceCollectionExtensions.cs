using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services for managing resources.
    /// </summary>
    public static IServiceCollection AddResourceManagement(this IServiceCollection services)
    {
        services.TryAddScoped<IResourceManager, ResourceManager>();

        return services;
    }

    /// <summary>
    /// Adds a service implementing <see cref="IConfigureOptions{ResourceManagementOptions}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the service implementing <see cref="IConfigureOptions{ResourceManagementOptions}"/> to.
    /// </typeparam>
    public static IServiceCollection AddResourceConfiguration<T>(this IServiceCollection services)
        where T : class, IConfigureOptions<ResourceManagementOptions>
        => services.AddTransient<IConfigureOptions<ResourceManagementOptions>, T>();
}
