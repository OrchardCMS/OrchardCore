using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Handlers;

/// <summary>
/// The <see cref="SiteCollectionExtensions"/> class is scheduled for removal in the next major release.
/// The <see cref="AddSiteDisplayDriver"/> extension method should be relocated 
/// to the <see cref="ServiceCollectionExtensions"/> class.
/// </summary>
public static class SiteCollectionExtensions
{
    public static IServiceCollection AddSiteDisplayDriver<TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<ISite>
        => services.AddDisplayDriver<ISite, TDriver>();
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDisplayDriver<TModel, TDriver>(this IServiceCollection services)
        where TDriver : class, IDisplayDriver<TModel>
        => services.AddScoped<IDisplayDriver<TModel>, TDriver>();
}
