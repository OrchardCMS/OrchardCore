using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Handlers;

/// <summary>
/// Rename this file to ServiceCollectionExtensions in v3.0.
/// </summary>
public static class SiteServiceCollectionExtensions
{
    public static IServiceCollection AddSiteDisplayDriver<TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<ISite>
        => services.AddDisplayDriver<ISite, TDriver>();

    public static IServiceCollection AddDisplayDriver<TModel, TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<TModel>
    {
        services.TryAddEnumerable(new[]
        {
            ServiceDescriptor.Scoped<IDisplayDriver<TModel>, TDriver>(),
        });

        return services;
    }
}
