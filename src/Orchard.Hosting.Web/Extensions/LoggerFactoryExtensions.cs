using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace Orchard.Hosting.Extensions
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddOrchardLogging(
            this ILoggerFactory loggingFactory,
            IServiceProvider serviceProvider)
        {
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
            foreach (var initiatorType in loggingInitiatorTypes)
            {
                loggerCollection.AddScoped(typeof(ILoggingInitiator), initiatorType);
            }
            var moduleServiceProvider = serviceProvider.CreateChildContainer(loggerCollection).BuildServiceProvider();
            foreach (var service in moduleServiceProvider.GetServices<ILoggingInitiator>())
            {
                service.Initialize(loggingFactory);
            }

            return loggingFactory;
        }
    }
}