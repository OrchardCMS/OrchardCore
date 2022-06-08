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
    public abstract class SiteStartupBase
    {
        public static ConcurrentDictionary<string, PermissionsContext> PermissionsContexts;

        static string[] _recipies = Array.Empty<string>();
        public static string[] Recipies { get { return _recipies; } set { _recipies = value ?? Array.Empty<string>(); } }

        static Assembly[] _assemblies = Array.Empty<Assembly>();
        public static Assembly[] Assemblies { get { return _assemblies; } set { _assemblies = value ?? Array.Empty<Assembly>(); } }

        static SiteStartupBase()
        {
            PermissionsContexts = new ConcurrentDictionary<string, PermissionsContext>();
        }

        protected virtual string[] SetupFeatures
        {
            get
            {
                return new string[] { "OrchardCore.Tenants" };
            }
        }

        protected virtual string[] TenantFeatures
        {
            get
            {
                return Array.Empty<String>();
            }
        }

        protected virtual void ConfigureOrchardServices(IServiceCollection collection)
        {

            collection.AddScoped<IRecipeHarvester, TestRecipeHarvester>(x => new TestRecipeHarvester(x.GetService<IRecipeReader>(), Recipies, Assemblies));

            collection.AddScoped<IAuthorizationHandler, PermissionContextAuthorizationHandler>(sp =>
            {
                return new PermissionContextAuthorizationHandler(sp.GetRequiredService<IHttpContextAccessor>(), PermissionsContexts);
            });
        }

        protected virtual void ConfigureAppBuilder(IApplicationBuilder appBuilder)
        {
            appBuilder.UseAuthorization();
        }

        public abstract Type WebStartupClass { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder =>
            {
                if (SetupFeatures != null && SetupFeatures.Length > 0)
                {
                    builder.AddSetupFeatures(SetupFeatures);
                };
                if (TenantFeatures != null && TenantFeatures.Length > 0)
                {
                    builder.AddTenantFeatures(TenantFeatures);
                };
                builder.ConfigureServices(collection =>
               {
                   ConfigureOrchardServices(collection);
               })
                .Configure(appBuilder => ConfigureAppBuilder(appBuilder));
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
