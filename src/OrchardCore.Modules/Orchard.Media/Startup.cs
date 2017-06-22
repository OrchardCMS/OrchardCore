using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.ContentManagement;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.Liquid;
using Orchard.Media.Filters;
using Orchard.Media.Models;
using Orchard.Media.Services;
using Orchard.StorageProviders.FileSystem;

namespace Orchard.Media
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMediaFileStore>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

                (string requestPath, string mediaPath) = GetSettings(env, shellOptions.Value, shellSettings);

                return new MediaFileStore(new FileSystemStore(mediaPath, requestPath));
            });

            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ContentPart, ImagePart>();
            services.AddMedia();

            services.AddScoped<ITemplateContextHandler, MediaFilters>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            (string requestPath, string mediaPath) = GetSettings(env, shellOptions.Value, shellSettings);

            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = requestPath,
                FileProvider = new PhysicalFileProvider(mediaPath)
            });
        }

        private (string requestPath, string mediaPath) GetSettings(IHostingEnvironment env, ShellOptions shellOptions, ShellSettings shellSettings)
        {
            var relativeMediaPath = Path.Combine(shellOptions.ShellsRootContainerName, shellOptions.ShellsContainerName, shellSettings.Name, "Media");

            return (
                requestPath: (string.IsNullOrEmpty(shellSettings.RequestUrlPrefix) ? "" : "/" + shellSettings.RequestUrlPrefix) + "/media", 
                mediaPath: env.ContentRootFileProvider.GetFileInfo(relativeMediaPath).PhysicalPath
                );
        }
    }
}
