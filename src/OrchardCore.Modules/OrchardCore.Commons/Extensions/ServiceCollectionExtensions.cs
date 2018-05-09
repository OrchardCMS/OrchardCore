using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commons.Filters;

namespace OrchardCore.Commons
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGeneratorTagFilter(this IServiceCollection services)
        {
            return services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MetaGeneratorFilter));
            });
        }
    }
}
