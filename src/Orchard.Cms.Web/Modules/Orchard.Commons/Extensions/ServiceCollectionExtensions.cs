using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Commons.Filters;

namespace Orchard.Commons
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
