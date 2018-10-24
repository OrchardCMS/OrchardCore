using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
                {
                    collection.AddScoped<IAuthorizationHandler, AlwaysLoggedInAuthHandler>();
                    collection.AddAuthentication((options) =>
                    {
                        options.AddScheme<AlwaysLoggedInApiAuthenticationHandler>("Api", null);
                    });
                }));
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

    public class AlwaysLoggedInApiAuthenticationHandler : AuthenticationHandler<ApiAuthorizationOptions>
    {
        public AlwaysLoggedInApiAuthenticationHandler(
            IOptionsMonitor<ApiAuthorizationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(
                AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new System.Security.Claims.ClaimsPrincipal(new StubIdentity()), "Api")));
        }
    }

    public class StubIdentity : IIdentity
    {
        public string AuthenticationType => "Dunno";

        public bool IsAuthenticated => true;

        public string Name => "Doug";
    }
}