using System;
using GraphQL;
using GraphQL.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Services;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver, RequestServicesDependencyResolver>();
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();

            services.AddGraphQLMutations();

            // Schema
            services.AddScoped<ISchemaService, SchemaService>();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddTransient<INavigationProvider, AdminMenu>();

            services.TryAddScoped<IGraphQLSchemaHashService, NullGraphQLSchemaHashService>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<GraphQLMiddleware>(new GraphQLSettings
            {
                BuildUserContext = ctx => new GraphQLUserContext
                {
                    User = ctx.User
                }
            });
        }
    }
}
