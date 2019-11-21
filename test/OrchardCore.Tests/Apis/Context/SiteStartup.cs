using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Security;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder =>
                builder.AddSetupFeatures(
                    "OrchardCore.Tenants"
                )
                .AddTenantFeatures(
                    "OrchardCore.Apis.GraphQL"
                )
                .ConfigureServices(collection =>
                {
                    collection.AddScoped<IAuthorizationHandler, AlwaysLoggedInAuthHandler>();
                    collection.AddAuthentication((options) =>
                    {
                        options.AddScheme<AlwaysLoggedInApiAuthenticationHandler>("Api", null);
                    });
                })
                .Configure(appBuilder => appBuilder.UseAuthorization()));

            services.AddSingleton<IModuleNamesProvider, ModuleNamesProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseOrchardCore();
        }

        private class ModuleNamesProvider : IModuleNamesProvider
        {
            private readonly string[] _moduleNames;

            public ModuleNamesProvider()
            {
                var assembly = Assembly.Load(new AssemblyName(typeof(Cms.Web.Startup).Assembly.GetName().Name));
                _moduleNames = assembly.GetCustomAttributes<ModuleNameAttribute>().Select(m => m.Name).ToArray();
            }

            public IEnumerable<string> GetModuleNames()
            {
                return _moduleNames;
            }
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
