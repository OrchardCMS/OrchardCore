using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Extensions;
using Orchard.Hosting.Web.Routing;

namespace Orchard.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureWebHost(
            this IApplicationBuilder builder,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddOrchardLogging(builder.ApplicationServices);

            // Add diagnostices pages
            // TODO: make this modules from configurations
            builder.UseRuntimeInfoPage();
            builder.UseDeveloperExceptionPage();

            // Add static files to the request pipeline.
            builder.UseStaticFiles();

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            builder.UseMiddleware<OrchardContainerMiddleware>();

            // Route the request to the correct tenant specific pipeline
            builder.UseMiddleware<OrchardRouterMiddleware>();

            // Load controllers
            var applicationPartManager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            var extensionManager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();
            foreach (var feature in extensionManager.AvailableFeatures())
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
            }

            return builder;
        }
    }
}