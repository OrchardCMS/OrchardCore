using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Recipes.Services;

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
                builder.AddSetupFeatures(
                    "OrchardCore.Tenants"
                )
                .AddTenantFeatures(
                    "OrchardCore.Apis.GraphQL"
                )
                .ConfigureServices(collection =>
                {
                    collection.AddScoped<IRecipeHarvester, TestRecipeHarvester>();

                    collection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
                    {
                        return new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(), PermissionsContexts);
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
}
