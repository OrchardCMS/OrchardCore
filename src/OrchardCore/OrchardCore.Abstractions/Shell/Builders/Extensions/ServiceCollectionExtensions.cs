using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a delegate to be invoked asynchronously just after a tenant container is created.
    /// </summary>
    public static IServiceCollection Initialize(this IServiceCollection services, Func<IServiceProvider, ValueTask> initializeAsync)
        => services.Configure<ShellContainerOptions>(options => options.Initializers.Add(initializeAsync));

    /// <summary>
    /// Registers a delegate used to configure asynchronously a type of options just after a tenant container is created.
    /// </summary>
    public static IServiceCollection Configure<TOptions>(
        this IServiceCollection services, Func<IServiceProvider, TOptions, ValueTask> configureAsync)
        where TOptions : class, IAsyncOptions, new()
    {
        if (!services.Any(d => d.ServiceType == typeof(TOptions)))
        {
            services.AddSingleton(new TOptions());
        }

        services.Initialize(async sp =>
        {
            var options = sp.GetRequiredService<TOptions>();
            await configureAsync(sp, options);
        });

        return services;
    }

    /// <summary>
    /// Registers an <see cref="IAsyncConfigureOptions{TOptions}"/> used to configure
    /// asynchronously a type of options just after a tenant container is created.
    /// </summary>
    public static IServiceCollection Configure<TOptions, TConfigure>(this IServiceCollection services)
        where TOptions : class, IAsyncOptions, new() where TConfigure : IAsyncConfigureOptions<TOptions>
    {
        if (!services.Any(d => d.ServiceType == typeof(TOptions)))
        {
            services.AddSingleton(new TOptions());
        }

        if (!services.Any(d => d.ServiceType == typeof(TConfigure)))
        {
            services.AddTransient(typeof(TConfigure));
            services.Initialize(async sp =>
            {
                var options = sp.GetRequiredService<TOptions>();
                var setup = sp.GetRequiredService<TConfigure>();
                await setup.ConfigureAsync(options);
            });
        }

        return services;
    }
}
