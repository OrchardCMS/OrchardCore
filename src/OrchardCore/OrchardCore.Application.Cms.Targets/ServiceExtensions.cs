using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;

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
            });

            services.ConfigureTenantServices<IDataProtectionProvider, IOptions<ShellOptions>, ShellSettings>(
                (collection, provider, options, settings) =>
            {
                var directory = Directory.CreateDirectory(Path.Combine(
                    options.Value.ShellsApplicationDataPath,
                    options.Value.ShellsContainerName,
                    settings.Name, "DataProtection-Keys"));

                collection.Add(new ServiceCollection()
                    .AddDataProtection()
                    .PersistKeysToFileSystem(directory)
                    .SetApplicationName(settings.Name)
                    .Services);
            });

            //services.AddTransient<IStartup, ServiceTest1>();
            //services.AddTransient<IStartup, ServiceTest2>();

            //services.ConfigureTenantServices<ShellSettings>((collection, settings) =>
            //{
            //    var test = settings.Name;

            // Called here in place of 'OC.Commons'
            //collection.AddLiquidViews();
            //});

            //services.ConfigureTenant((builder, routes, serviceProvider) =>
            //{
            //    var test = serviceProvider.GetRequiredService<ShellSettings>().Name;
            //    builder.UseMiddleware<NonBlockingMiddleware>();
            //});

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

    public class NonBlockingMiddleware
    {
        private readonly RequestDelegate _next;

        public NonBlockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("OrchardCore", "2.0");
            await _next.Invoke(httpContext);
        }
    }
}
