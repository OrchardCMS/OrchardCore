using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Commons.Filters;

namespace Orchard.Commons
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResultFilters(this IServiceCollection services)
        {
            services.AddSingleton<IFilterMetadata, MetaGeneratorFilter>();

            return services;
        }
    }
}