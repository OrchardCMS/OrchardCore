using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.JsonApi.Filters;

namespace Orchard.ContentManagement.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiContentManagementDisplay(this IServiceCollection services)
        {
            services.TryAddScoped<IApiContentManager, ApiContentManager>();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(JsonApiFilter));
            });

            return services;
        }
    }
}
