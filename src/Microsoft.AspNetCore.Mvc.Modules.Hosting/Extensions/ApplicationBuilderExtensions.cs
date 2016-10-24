using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Hosting;
using Orchard.Hosting.Web.Routing;
using Orchard.Environment.Extensions.Loaders;

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
            var availableExtensions = extensionManager.GetExtensions();
            foreach (var extension in availableExtensions)
            {
                var contentPath = Path.Combine(extension.ExtensionFileInfo.PhysicalPath, "Content");
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
                var extensionEntries = extensionManager.LoadExtensions(availableExtensions);

                foreach (var assemblyPart in extensionEntries
                    .Where(x => x.GetType() != typeof(FailedExtensionEntry))
                    .Select(x => new AssemblyPart(x.Assembly))) {
                    applicationPartManager.ApplicationParts.Add(assemblyPart);
                }

                var failed = extensionEntries.Where(x => x.GetType() == typeof(FailedExtensionEntry));

                foreach (FailedExtensionEntry failure in failed)
                {
                    logger.LogCritical("Could not load an extension", failure.Exception);
                }
            }

            return builder;
        }
    }
}