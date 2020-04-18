using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Contents.Services;

namespace OrchardCore.Contents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentQueries(this IServiceCollection services)
        {
            services.AddScoped<IContentQueryService, ContentQueryService>();            
            return services;
        }
    }
}
