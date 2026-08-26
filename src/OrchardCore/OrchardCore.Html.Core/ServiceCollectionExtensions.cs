using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Html.Services;

namespace OrchardCore.Html;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHtmlCore(this IServiceCollection services)
    {
        services.TryAddScoped<IHtmlDisplayService, HtmlDisplayService>();

        return services;
    }
}
