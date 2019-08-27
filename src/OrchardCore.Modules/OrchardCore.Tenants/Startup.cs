using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Setup;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSetup();
        }
    }

    [Feature("OrchardCore.Tenants.FileProvider")]
    public class FileProviderStartup : StartupBase
    {
        /// <summary>
        /// The path in the tenant's App_Data folder containing the files
        /// </summary>
        private const string AssetsPath = "wwwroot";

        // Run after other middlewares
        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITenantFileProvider>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                string contentRoot = GetContentRoot(shellOptions.Value, shellSettings);

                if (!Directory.Exists(contentRoot))
                {
                    Directory.CreateDirectory(contentRoot);
                }
                return new TenantFileProvider(contentRoot);
            });

            services.AddSingleton<IStaticFileProvider>(serviceProvider =>
            {
                return serviceProvider.GetRequiredService<ITenantFileProvider>();
            });
        }
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var tenantFileProvider = serviceProvider.GetRequiredService<ITenantFileProvider>();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = tenantFileProvider,
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true,

                // Cache the tenant static files for 7 days
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=2592000, s-max-age=31557600";
                }
            });
        }

        private string GetContentRoot(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, AssetsPath);
        }
    }
}
