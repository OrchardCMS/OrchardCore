using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.RestApis.Filters;
using OrchardCore.RestApis.Queries;
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

        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();
            services.AddScoped<ContentItemType>();
            services.AddScoped<ContentTypeType>();
            services.AddScoped<ContentType>();

            services.AddScoped<TitlePartType>();
            services.AddScoped<AutoRoutePartType>();

            services.AddScoped<IObjectGraphType, TitlePartType>();
            services.AddScoped<IObjectGraphType, AutoRoutePartType>();
            

            services.AddScoped<ContentPartInterface>();
            
            services.AddScoped<ISchema, ContentSchema>();

            return services;
        }
    }
}
