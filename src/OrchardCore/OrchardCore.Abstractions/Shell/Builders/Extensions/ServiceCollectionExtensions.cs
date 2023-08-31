using System;
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
}
