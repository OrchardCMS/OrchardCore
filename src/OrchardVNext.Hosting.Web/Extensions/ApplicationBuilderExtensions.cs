using Microsoft.AspNet.Builder;
using Microsoft.Framework.Logging;
using OrchardVNext.Hosting.Web.Routing;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.Extensions.Loaders;
using OrchardVNext.Hosting.Extensions;
using OrchardVNext.Abstractions.Logging;
using System.Reflection;
using System;

namespace OrchardVNext.Hosting {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder ConfigureWeb(
            [NotNull] this IApplicationBuilder builder,
            [NotNull] ILoggerFactory loggerFactory) {

            var loader = builder.ApplicationServices.GetRequiredService<IExtensionLoader>();
            var manager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();

            var descriptor = manager.GetExtension("OrchardVNext.Logging.Console");
            var entry = loader.Load(descriptor);
            var info = entry.Assembly.GetType("OrchardVNext.Logging.LoggingInitiator");
            var instance = Activator.CreateInstance(info);
            info.GetMethod("Initialize").Invoke(instance, new[] { loggerFactory });

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