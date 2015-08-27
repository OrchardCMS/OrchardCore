using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Orchard.Hosting.Extensions;
using Orchard.Hosting.Web.Routing;
using System;

namespace Orchard.Hosting {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder ConfigureWebHost(
            [NotNull] this IApplicationBuilder builder,
            [NotNull] ILoggerFactory loggerFactory) {

            loggerFactory.AddOrchardLogging(builder.ApplicationServices);
            
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