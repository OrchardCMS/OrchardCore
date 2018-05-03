using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell.Data;
using Microsoft.AspNetCore.DataProtection;

using OrchardCore.Environment.Shell;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

using OrchardCore.Modules;
using OrchardCore.DisplayManagement.Liquid;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            services.AddThemingHost();
            services.AddManifestDefinition("theme");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAuthentication();
            services.AddModules(modules =>
            {
                modules.WithDefaultFeatures(
                    "OrchardCore.Antiforgery", "OrchardCore.Mvc", "OrchardCore.Settings",
                    "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons");

                modules.Configure(collection =>
                {
                    //collection.AddTransient<IStartup, ServiceTest1>();
                    //collection.AddTransient<IStartup, ServiceTest2>();
                });
            });

            services.AddTransient<IStartup, ServiceTest1>();
            services.AddTransient<IStartup, ServiceTest2>();

            services.ConfigureTenantServices<ShellSettings>((collection, settings) =>
            {
                // Here we retrieve the tenant name.
                var test = settings.Name;

                // It works by calling the following here.
                // In place of calling it in 'OC.Commons'
                collection.AddLiquidViews();
            });

            services.ConfigureTenant((builder, routes, sp) =>
            {
                ;
            });

            return services;
        }
    }

    public class ServiceTest1 : StartupBase
    {
        private readonly ShellSettings _shellSettings;

        public ServiceTest1(
            ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override int Order => 1000;

        public override void ConfigureServices(IServiceCollection services)
        {
            var test = _shellSettings.Name;
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            ;
        }
    }

    public class ServiceTest2 : StartupBase
    {
        private readonly ShellSettings _shellSettings;

        public ServiceTest2(
            ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override int Order => 1000;

        public override void ConfigureServices(IServiceCollection services)
        {
            var test = _shellSettings.Name;
        }
    }
}
