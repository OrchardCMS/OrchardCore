using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a service implementing <see cref="IConfigureOptions{ResourceManagementOptions}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the implementation of <see cref="IConfigureOptions{ResourceManagementOptions}"/> to register.
    /// </typeparam>
    public static IServiceCollection AddResourceConfiguration<T>(this IServiceCollection services)
        where T : class, IConfigureOptions<ResourceManagementOptions>
        => services.AddTransient<IConfigureOptions<ResourceManagementOptions>, T>();
}
