using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQL();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddTransient<INavigationProvider, AdminMenu>();
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
