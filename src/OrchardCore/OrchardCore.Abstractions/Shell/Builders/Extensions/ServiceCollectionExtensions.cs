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
    /// Registers an initialization delegate that will be executed asynchronously on each tenant container creation.
    /// </summary>
    public static IServiceCollection AddAsyncInitialization(this IServiceCollection services, Func<IServiceProvider, Task> _initializeAsync) =>
            services.AddTransient<IShellAsyncInitializer>(sp => new ShellAsyncInitializer(_initializeAsync));

}
