using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ApplicationBuilderExtensions
    {
        public static ModularApplicationBuilder UseMvcModules(this ModularApplicationBuilder modularApp)
        {
            modularApp.Configure(app =>
            {
                app.UseMvcModules();
            });

            return modularApp;
        }

        public static IApplicationBuilder UseMvcModules(this IApplicationBuilder app)
        {
            var extensionManager = app.ApplicationServices.GetRequiredService<IExtensionManager>();
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            // TODO: configure the location and parameters (max-age) per module.
            var availableExtensions = extensionManager.GetExtensions();
            foreach (var extension in availableExtensions)
            {
                var contentPath = Path.Combine(extension.ExtensionFileInfo.PhysicalPath, "Content");
                if (Directory.Exists(contentPath))
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = "/" + extension.Id,
                        FileProvider = new PhysicalFileProvider(contentPath)
                    });
                }
            }

            return app;
        }
    }
}