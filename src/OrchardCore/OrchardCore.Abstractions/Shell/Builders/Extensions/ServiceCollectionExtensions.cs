using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShellContainerAsyncInitializer(
        this IServiceCollection services, Func<IServiceProvider, Task> _initializeAsync)
    {
        services.AddSingleton<IShellContainerAsyncInitializer>(
            sp => new ShellContainerAsyncInitializer(_initializeAsync));

        return services;
    }
}
