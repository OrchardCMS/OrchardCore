using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.RestApis.Filters;
using Orchard.RestApis.Queries;
using Orchard.RestApis.Types;

namespace Orchard.RestApis
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

        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();
            services.AddScoped<ContentItemType>();
            services.AddScoped<ContentTypeType>();
            services.AddScoped<ContentQuery>();
            services.AddScoped<ISchema, ContentSchema>();

            return services;
        }
    }
}
