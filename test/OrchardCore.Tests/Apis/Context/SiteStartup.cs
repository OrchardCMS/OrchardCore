using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Security;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder =>
                builder.AddGlobalFeatures(
                    "OrchardCore.Tenants",
                    "OrchardCore.Apis.GraphQL"
                )
                .ConfigureServices(collection =>
                    collection.AddScoped<IAuthorizationHandler, AlwaysLoggedInAuthHandler>()
                ));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseOrchardCore();
        }
    }

    public class AlwaysLoggedInAuthHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}