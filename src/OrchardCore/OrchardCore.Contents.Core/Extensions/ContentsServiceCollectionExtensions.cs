using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Core.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ContentsServiceCollectionExtensions
{
    public static IServiceCollection AddContentServices(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<MvcOptions>, MvcOptionsConfiguration>();

        return services;
    }
}
