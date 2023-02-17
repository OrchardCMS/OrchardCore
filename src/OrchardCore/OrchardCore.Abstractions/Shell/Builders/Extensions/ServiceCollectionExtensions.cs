using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// Provides a mechanism for initializing tenant container services asynchronously.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an async initializer being called on each tenant container creation.
    /// </summary>
    public static IServiceCollection AddShellContainerAsyncInitializer(
        this IServiceCollection services, Func<IServiceProvider, Task> _initializeAsync)
    {
        services.AddTransient<IShellContainerAsyncInitializer>(
            sp => new ShellContainerAsyncInitializer(_initializeAsync));

        return services;
    }
}
