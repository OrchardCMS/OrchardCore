using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Extensions;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.ShellBuilders.Models;
using Microsoft.Framework.Logging;

#if DNXCORE50
using System.Reflection;
#endif

namespace OrchardVNext.Hosting.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ShellContainerFactory(IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) {
            _serviceProvider = serviceProvider;
            _logger = loggerFactory.CreateLogger<ShellContainerFactory>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();

            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);

            foreach (var dependency in blueprint.Dependencies
                .Where(x => x.Type.Name == "ShellStartup" || 
                            x.Type.Name == (settings.Name + "ShellStartup"))) {

                _logger.LogDebug("Shell Startup: {0}", dependency.Type);

                // TODO: Rewrite to get rid of reflection.
                var instance = Activator.CreateInstance(dependency.Type);
                var info = instance.GetType();
                info.GetMethod("ConfigureServices").Invoke(instance, new[] { serviceCollection });
            }

            foreach (var dependency in blueprint.Dependencies
                .Where(t => typeof (IModule).IsAssignableFrom(t.Type))) {

                _logger.LogDebug("IModule Type: {0}", dependency.Type);

                // TODO: Rewrite to get rid of reflection.
                var instance = (IModule) Activator.CreateInstance(dependency.Type);
                instance.Configure(serviceCollection);
            }

            foreach (var dependency in blueprint.Dependencies
                .Where(t => !typeof(IModule).IsAssignableFrom(t.Type)))
            {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf)))
                {
                    _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else
                    {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            return new WrappingServiceProvider(_serviceProvider, serviceCollection);
        }

        private class WrappingServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _services;

            // Need full wrap for generics like IOptions
            public WrappingServiceProvider(IServiceProvider fallback, IServiceCollection replacedServices)
            {
                var services = new ServiceCollection();
                var manifest = fallback.GetRequiredService<IRuntimeServices>();
                foreach (var service in manifest.Services) {
                    services.AddTransient(service, sp => fallback.GetService(service));
                }
                
                services.AddSingleton<IRuntimeServices>(sp => new HostingManifest(services));
                services.Add(replacedServices);

                _services = services.BuildServiceProvider();
            }

            public object GetService(Type serviceType) {
                return _services.GetService(serviceType);
            }


            // Manifest exposes the fallback manifest in addition to ITypeActivator, IHostingEnvironment, and ILoggerFactory
            private class HostingManifest : IRuntimeServices {
                public HostingManifest(IServiceCollection hostServices) {
                    Services = hostServices.Select(s => s.ServiceType).Distinct();
                }

                public IEnumerable<Type> Services { get; private set; }
            }
        }
    }
}