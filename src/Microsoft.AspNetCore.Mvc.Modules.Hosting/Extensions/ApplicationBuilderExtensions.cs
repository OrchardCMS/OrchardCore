using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Hosting;
using Orchard.Hosting.Web.Routing;

namespace Microsoft.AspNetCore.Mvc.Modules.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseModules(this IApplicationBuilder builder)
        {
            var extensionManager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();
            var hostingEnvironment = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var loggerFactory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Default");

            if (hostingEnvironment.IsDevelopment())
            {
                builder.UseDeveloperExceptionPage();
            }

            // Add static files to the request pipeline.
            builder.UseStaticFiles();

            // TODO: configure the location and parameters (max-age) per module.
            foreach(var extension in extensionManager.AvailableExtensions())
            {
                var contentPath = Path.Combine(hostingEnvironment.ContentRootPath, extension.Location, extension.Id, "Content");
                if (Directory.Exists(contentPath))
                {
                    builder.UseStaticFiles(new StaticFileOptions()
                    {
                        RequestPath = "/" + extension.Id,
                        FileProvider = new PhysicalFileProvider(contentPath)
                    });
                }
            }

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            builder.UseMiddleware<OrchardContainerMiddleware>();

            // Route the request to the correct tenant specific pipeline
            builder.UseMiddleware<OrchardRouterMiddleware>();

            // Load controllers
            var applicationPartManager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            using (logger.BeginScope("Loading extensions"))
            {
                Parallel.ForEach(extensionManager.AvailableFeatures(), feature =>
                {
                    try
                    {
                        var extensionEntry = extensionManager.LoadExtension(feature.Extension);
                        applicationPartManager.ApplicationParts.Add(new AssemblyPart(extensionEntry.Assembly));
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical("Could not load an extension", feature.Extension, e);
                    }
                });
            }

            return builder;
        }
    }
}