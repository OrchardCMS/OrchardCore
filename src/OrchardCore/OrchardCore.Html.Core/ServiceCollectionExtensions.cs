using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Html.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHtmlServices(this IServiceCollection services)
    {
        services.TryAddScoped<IHtmlDisplayService, HtmlDisplayService>();

        return services;
    }
}
