using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Tests.Apis
{
    public class SiteStartup
    {
        private readonly TestSiteConfiguration _configuration;

        public SiteStartup(TestSiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder =>
                builder.AddGlobalFeatures(
                    "OrchardCore.Apis.GraphQL",
                    "OrchardCore.Apis.JsonApi",
                    "OrchardCore.Apis.OpenApi"
                ));

            services.AddScoped<IAuthorizationHandler, AlwaysLoggedInAuthHandler>();

            services
                .Configure<ShellOptions>(options => options.ShellsContainerName = _configuration.ShellsContainerName);
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
