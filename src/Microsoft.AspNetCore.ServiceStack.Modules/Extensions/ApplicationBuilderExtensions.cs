using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Funq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using ServiceStack;

namespace Microsoft.AspNetCore.ServiceStack.Modules
{
    public static class ApplicationBuilderExtensions
    {
        public static ModularApplicationBuilder UseServiceStackModules(this ModularApplicationBuilder modularApp)
        {
            modularApp.Configure(app =>
            {
                var extensionManager = app.ApplicationServices.GetRequiredService<IExtensionManager>();
                var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Default");

                //// Route the request to the correct tenant specific pipeline
                //app.UseMiddleware<OrchardRouterMiddleware>();
                
                ConcurrentBag<Assembly> badOfTypes
                    = new ConcurrentBag<Assembly>();

                var availableExtensions = extensionManager.GetExtensions();
                using (logger.BeginScope("Loading extensions"))
                {
                    Parallel.ForEach(availableExtensions, new ParallelOptions { MaxDegreeOfParallelism = 4 }, ae =>
                    {
                        try
                        {
                            var extensionEntry = extensionManager
                                .LoadExtensionAsync(ae)
                                .GetAwaiter()
                                .GetResult();

                            if (!extensionEntry.IsError)
                            {
                                badOfTypes.Add(extensionEntry.Assembly);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogCritical("Could not load an extension", ae, e);
                        }
                    });
                }

                app.UseServiceStack(new AppHost("Orchard", badOfTypes.ToArray()));
            });

            return modularApp;
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost(string serviceName, params Assembly[] assembliesWithServices) : 
            base(serviceName, assembliesWithServices)
        {
        }

        public override void Configure(Container container)
        {
        }
    }
}