using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Handlers;

public static class SiteServiceCollectionExtensions
{
    public static IServiceCollection AddSiteDisplayDriver<TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<ISite>
    {
        return services.AddScoped<IDisplayDriver<ISite>, TDriver>();
    }
}
