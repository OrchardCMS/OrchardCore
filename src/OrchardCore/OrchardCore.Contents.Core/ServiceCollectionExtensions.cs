using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Core.Services;

namespace OrchardCore.Contents.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContentServices(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<MvcOptions>, MvcOptionsConfiguration>();

        return services;
    }
}
