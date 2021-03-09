using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Security;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Lucene
{
    public class SiteStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddOrchardCms(builder =>
                builder.AddSetupFeatures(
                    "OrchardCore.Tenants"
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

            services.Replace(ServiceDescriptor.Transient<IConfigureOptions<ShellOptions>, LuceneShellOptionsSetup>());

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
                Assembly assembly = Assembly.Load(new AssemblyName(typeof(Cms.Web.Startup).Assembly.GetName().Name));
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

    public class LuceneShellOptionsSetup : IConfigureOptions<ShellOptions>
    {
        private const string DefaultAppDataPath = "Lucene_App_Data";
        private const string DefaultSitesPath = "Sites";

        private readonly IHostEnvironment _hostingEnvironment;

        public LuceneShellOptionsSetup(IHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(ShellOptions options)
        {
            options.ShellsApplicationDataPath = Path.Combine(_hostingEnvironment.ContentRootPath, DefaultAppDataPath);
            options.ShellsContainerName = DefaultSitesPath;
        }
    }
}
