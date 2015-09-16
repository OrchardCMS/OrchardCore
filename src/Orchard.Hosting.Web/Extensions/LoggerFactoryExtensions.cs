using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Orchard.Abstractions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using System;
using System.Linq;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Hosting.Extensions {
    public static class LoggerFactoryExtensions {
        public static ILoggerFactory AddOrchardLogging(
            [NotNull] this ILoggerFactory loggingFactory, 
            IServiceProvider serviceProvider) {
            /* TODO (ngm): Abstract this logger stuff outta here! */
            var loader = serviceProvider.GetRequiredService<IExtensionLoader>();
            var manager = serviceProvider.GetRequiredService<IExtensionManager>();

            var descriptor = manager.GetExtension("Orchard.Logging.Console");
            var entry = loader.Load(descriptor);
            var loggingInitiatorTypes = entry
                .Assembly
                .ExportedTypes
                .Where(et => typeof(ILoggingInitiator).IsAssignableFrom(et));

            foreach (var initiatorType in loggingInitiatorTypes) {
                var instance = (ILoggingInitiator)ActivatorUtilities
                    .CreateInstance(serviceProvider, initiatorType);
                instance.Initialize(loggingFactory);
            }

            return loggingFactory;
        }
    }
}
