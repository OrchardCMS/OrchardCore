using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Web.Routing;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ApplicationBuilderExtensions
    {
        public static ModularApplicationBuilder UseMvcModules(this ModularApplicationBuilder modularApp)
        {
            modularApp.Configure(app =>
            {
                var extensionManager = app.ApplicationServices.GetRequiredService<IExtensionManager>();
                var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Default");

                // Route the request to the correct tenant specific pipeline
                app.UseMiddleware<OrchardRouterMiddleware>();

                // Load controllers
                var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();

                var availableExtensions = extensionManager.GetExtensions();
                using (logger.BeginScope("Loading extensions"))
                {
                    Parallel.ForEach(availableExtensions, new ParallelOptions { MaxDegreeOfParallelism = 4 }, ae =>
                    {
                        try
                        {
                            var extensionEntry = extensionManager.LoadExtensionAsync(ae).Result;

                            if (!extensionEntry.IsError)
                            {
                                var assemblyPart = new AssemblyPart(extensionEntry.Assembly);
                                applicationPartManager.ApplicationParts.Add(assemblyPart);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogCritical("Could not load an extension", ae, e);
                        }
                    });
                }

            });

            return modularApp;
        }
    }
}