using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Orchard.Abstractions.Logging;
using Orchard.Hosting.Extensions;
using Orchard.Hosting.Extensions.Loaders;
using Orchard.Hosting.Web.Routing;
using System;
using System.Linq;

#if DNXCORE50
using System.Reflection;
using Orchard.DependencyInjection;
#endif

namespace Orchard.Hosting {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder ConfigureWebHost(
            [NotNull] this IApplicationBuilder builder,
            [NotNull] ILoggerFactory loggerFactory) {


            /* TODO (ngm): Abstract this logger stuff outta here! */
            var loader = builder.ApplicationServices.GetRequiredService<IExtensionLoader>();
            var manager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();

            var descriptor = manager.GetExtension("Orchard.Logging.Console");
            var entry = loader.Load(descriptor);
            var loggingInitiatorTypes = entry
                .Assembly
                .ExportedTypes
                .Where(et => typeof(ILoggingInitiator).IsAssignableFrom(et));

            foreach (var initiatorType in loggingInitiatorTypes) {
                var instance = (ILoggingInitiator)ActivatorUtilities
                    .CreateInstance(builder.ApplicationServices, initiatorType);
                instance.Initialize(loggerFactory);
            }


            //builder.UseMiddleware<OrchardLoggingMiddleware>();

            builder.UseMiddleware<OrchardContainerMiddleware>();
            builder.UseMiddleware<OrchardShellHostMiddleware>();

            // Think this needs to be inserted in a different part of the pipeline, possibly
            // when DI is created for the shell
            builder.UseMiddleware<OrchardRouterMiddleware>();

            return builder;
        }

        public static void InitializeHost([NotNull] this IApplicationBuilder builder) {
            var host = builder.ApplicationServices.GetRequiredService<IOrchardHost>();
            host.Initialize();
        }
    }
}