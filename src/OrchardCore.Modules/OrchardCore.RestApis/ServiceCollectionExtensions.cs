using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.RestApis.Filters;
using OrchardCore.RestApis.Queries;
using OrchardCore.RestApis.Queries.Types;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonApi(this IServiceCollection services)
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
