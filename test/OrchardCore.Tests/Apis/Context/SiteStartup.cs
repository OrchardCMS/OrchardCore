using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tenants;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteStartup
    {
        public static ConcurrentDictionary<string, PermissionsContext> PermissionsContexts;

        static SiteStartup()
        {
            PermissionsContexts = new ConcurrentDictionary<string, PermissionsContext>();
        }

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

    public class PermissionsContext 
    {
        public IEnumerable<Permission> AuthorizedPermissions { get; set; } = Enumerable.Empty<Permission>();

        public bool UsePermissionsContext { get; set; } = false;
    }

    public class AlwaysLoggedInAuthHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionsContext _permissionsContext; 

        public AlwaysLoggedInAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _permissionsContext = new PermissionsContext();

            var requestContext = httpContextAccessor.HttpContext.Request;

            if (requestContext?.Headers.ContainsKey("PermissionsContext") == true &&
                SiteStartup.PermissionsContexts.TryGetValue(requestContext.Headers["PermissionsContext"], out var permissionsContext))
            {
                _permissionsContext = permissionsContext;
            }
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = (_permissionsContext.AuthorizedPermissions ?? Enumerable.Empty<Permission>()).ToList();
            permissions.AddRange(new[] { Permissions.ManageTenants });

            if (!_permissionsContext.UsePermissionsContext)
            {
                context.Succeed(requirement);
            }
            else if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

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