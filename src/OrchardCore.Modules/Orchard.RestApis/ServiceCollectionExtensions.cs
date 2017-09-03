using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.RestApis.Filters;

namespace Orchard.RestApis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiContentManagementDisplay(this IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(JsonApiFilter));
            });

            services.TryAddScoped<IApiContentManager, ApiContentManager>();

            return services;
        }
    }
}
