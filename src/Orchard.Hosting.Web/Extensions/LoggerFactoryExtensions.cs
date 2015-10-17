using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using System;
using System.Linq;
using Orchard.Environment.Shell.Builders;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Hosting.Extensions {
    public static class LoggerFactoryExtensions {
        public static ILoggerFactory AddOrchardLogging(
            this ILoggerFactory loggingFactory, 
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

            IServiceCollection loggerCollection = new ServiceCollection();
            foreach (var initiatorType in loggingInitiatorTypes) {
                loggerCollection.AddScoped(typeof(ILoggingInitiator), initiatorType);
            }
            var moduleServiceProvider = loggerCollection.BuildShellServiceProviderWithHost(serviceProvider);
            foreach (var service in moduleServiceProvider.GetServices<ILoggingInitiator>()) {
                service.Initialize(loggingFactory);
            }

            return loggingFactory;
        }
    }
}
