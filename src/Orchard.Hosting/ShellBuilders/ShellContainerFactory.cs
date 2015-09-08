using System;
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Orchard.Configuration.Environment;
using Orchard.DependencyInjection;
using Orchard.Hosting.ShellBuilders.Models;
using Microsoft.Framework.Logging;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Hosting.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ShellContainerFactory(IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) {
            _serviceProvider = serviceProvider;
            _logger = loggerFactory.CreateLogger<ShellContainerFactory>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            IServiceCollection serviceCollection = new ServiceCollection();
            
            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);

            foreach (var dependency in blueprint.Dependencies
                .Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {

                _logger.LogDebug("IModule Type: {0}", dependency.Type);

                ((IModule)ActivatorUtilities
                    .CreateInstance(_serviceProvider, dependency.Type))
                    .Configure(serviceCollection);
            }

            foreach (var dependency in blueprint.Dependencies
                .Where(t => !typeof(IModule).IsAssignableFrom(t.Type))) {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                    _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            return serviceCollection.BuildShellServiceProviderWithHost(_serviceProvider);
        }
    }
}