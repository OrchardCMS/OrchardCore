using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class DisplayDriverServiceCollectionExtensions
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
