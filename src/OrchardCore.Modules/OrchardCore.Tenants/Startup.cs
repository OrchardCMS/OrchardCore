using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Tenants
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<INavigationProvider, AdminMenu>();
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
        
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

            string contentRoot = GetContentRoot(shellOptions.Value, shellSettings);

            if (!Directory.Exists(contentRoot))
            {
                Directory.CreateDirectory(contentRoot);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(contentRoot),
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true
            });
        }

        private string GetContentRoot(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, AssetsPath);
        }
    }
}
