using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Secrets;

/// <summary>
/// Extension methods for setting up Secrets services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core Secrets services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSecrets(this IServiceCollection services)
    {
        services.AddScoped<ISecretManager, SecretManager>();

        return services;
    }

    /// <summary>
    /// Adds a secret store to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of secret store to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSecretStore<T>(this IServiceCollection services) where T : class, ISecretStore
    {
        services.AddScoped<ISecretStore, T>();

        return services;
    }
}
