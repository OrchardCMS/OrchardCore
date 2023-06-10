using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a delegate to be invoked asynchronously after a tenant container is created.
    /// </summary>
    public static IServiceCollection Initialize(this IServiceCollection services, Func<IServiceProvider, ValueTask> _initializeAsync)
        => services.Configure<ShellContainerOptions>(options => options.Initializers.Add(_initializeAsync));

    /// <summary>
    /// Retrieves the singleton implementation types from the provided service collection.
    /// </summary>
    public static IEnumerable<Type> GetSingletonImplementationTypes(this IServiceCollection services) =>
        services
            .Where(sd => sd.Lifetime == ServiceLifetime.Singleton)
            .Select(sd => sd.GetImplementationType());

}
