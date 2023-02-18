using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers an initialization delegate to be executed asynchronously on tenant container creation.
    /// </summary>
    public static IServiceCollection Initialize(this IServiceCollection services, Func<IServiceProvider, ValueTask> _initializeAsync) =>
            services.Configure<ShellContainerOptions>(options => options.AsyncInitializations.Add(_initializeAsync));
}
