using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds tenant level services.
    /// </summary>
    public static IServiceCollection AddSetup(this IServiceCollection services)
    {
        services.TryAddScoped<ISetupService, SetupService>();

        services.TryAddSingleton<ISetupUserIdGenerator, SetupUserIdGenerator>();

        return services;
    }
}
