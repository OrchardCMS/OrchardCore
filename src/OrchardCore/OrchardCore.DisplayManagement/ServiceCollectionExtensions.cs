using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSiteDisplayDriver<TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<ISite>
        => services.AddDisplayDriver<ISite, TDriver>();

    public static IServiceCollection AddDisplayDriver<TModel, TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<TModel>
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDisplayDriver<TModel>, TDriver>());

        return services;
    }
}
