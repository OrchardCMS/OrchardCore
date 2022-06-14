using System;
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

namespace OrchardCore.Testing.Context
{
    public class SiteStartup
    {
        public static ConcurrentDictionary<string, PermissionsContext> PermissionsContexts = new ConcurrentDictionary<string, PermissionsContext>();

        public static Type WebStartupClass;

        public static readonly List<RecipeLocator> Recipies = new List<RecipeLocator>();

        public static readonly List<string> TenantFeatures = new List<string>();

        public static readonly List<string> AdditionalSetupFeatures = new List<string>();

        public static Action<IServiceCollection> ConfigureOrchardServices = (collection) =>
        {
            collection.AddScoped<IRecipeHarvester, TestRecipeHarvester>(x => new TestRecipeHarvester(x.GetService<IRecipeReader>(), Recipies));

            collection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
            {
                return new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(), PermissionsContexts);
            });
        };

        public static Action<IApplicationBuilder> ConfigureAppBuilder = (appBuilder) =>
        {
            appBuilder.UseAuthorization();
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder =>
            {
                var setupFeatures = new string[] { "OrchardCore.Tenants" };

                if (AdditionalSetupFeatures != null)
                {
                    setupFeatures = setupFeatures.Concat(AdditionalSetupFeatures).ToArray();
                };
                builder.AddSetupFeatures(setupFeatures);


                if (TenantFeatures != null)
                {
                    builder.AddTenantFeatures(TenantFeatures.ToArray());
                };

                builder.ConfigureServices(collection =>
               {
                   if (ConfigureOrchardServices != null)
                   {
                       ConfigureOrchardServices(collection);
                   }
               });

                if (ConfigureAppBuilder != null)
                {
                    builder.Configure(appBuilder => ConfigureAppBuilder(appBuilder));
                }
            });

            services.AddSingleton<IModuleNamesProvider, ModuleNamesProvider>(x => new ModuleNamesProvider(WebStartupClass));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseOrchardCore();
        }

        private class ModuleNamesProvider : IModuleNamesProvider
        {
            private readonly string[] _moduleNames;

            public ModuleNamesProvider(Type webStartupType)
            {
                var assembly = Assembly.Load(new AssemblyName(webStartupType.Assembly.GetName().Name));
                _moduleNames = assembly.GetCustomAttributes<ModuleNameAttribute>().Select(m => m.Name).ToArray();
            }

            public IEnumerable<string> GetModuleNames()
            {
                return _moduleNames;
            }
        }
    }
}
