using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.DotNet.Cli.Utils;
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
        public static IApplicationBuilder UseModules(
            this IApplicationBuilder builder,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
        {
            // TODO: Load logger configuration
            // loggerFactory.AddConsole(LogLevel.Error);

            var extensionManager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();

            // Add diagnostices pages
            // TODO: make this modules from configurations
            // builder.UseRuntimeInfoPage(); // removed!
            builder.UseDeveloperExceptionPage();

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

            var sw = Stopwatch.StartNew();

            Parallel.ForEach(extensionManager.AvailableFeatures(), feature =>
            {
                try
                {
                    var extensionEntry = extensionManager.LoadExtension(feature.Extension);
                    applicationPartManager.ApplicationParts.Add(new AssemblyPart(extensionEntry.Assembly));
                }
                catch
                {
                    // TODO: An extension couldn't be loaded, log
                }
            });

            // TODO: Why isn't it using the logger ?

            var message = $"Overall time to dynamically compile and load extensions: {sw.Elapsed}";

            if (Debugger.IsAttached)
            {
                Debug.WriteLine(message);
            }
            else
            {
                Reporter.Output.WriteLine(message);
                Reporter.Output.WriteLine();
            }

            return builder;
        }
    }
}