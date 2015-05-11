using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders.Models;
using OrchardVNext.Mvc;
using OrchardVNext.Routing;
#if DNXCORE50
using System.Reflection;
#endif

namespace OrchardVNext.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ShellContainerFactory(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IOrchardShell, DefaultOrchardShell>();
            serviceCollection.AddScoped<IRouteBuilder, DefaultShellRouteBuilder>();
            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);
            
            foreach (var dependency in blueprint.Dependencies
                .Where(t => typeof (IModule).IsAssignableFrom(t.Type))) {

                Logger.Debug("IModule Type: {0}", dependency.Type);

                // TODO: Rewrite to get rid of reflection.
                var instance = (IModule) Activator.CreateInstance(dependency.Type);
                instance.Configure(serviceCollection);
            }

            var p = _serviceProvider.GetService<IOrchardLibraryManager>();
            serviceCollection.AddInstance<IAssemblyProvider>(new DefaultAssemblyProviderTest(p, _serviceProvider, _serviceProvider.GetService<IAssemblyLoaderContainer>()));

            foreach (var dependency in blueprint.Dependencies
                .Where(t => !typeof(IModule).IsAssignableFrom(t.Type)))
            {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf)))
                {
                    Logger.Debug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

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

            serviceCollection.AddLogging();

            return new WrappingServiceProvider(_serviceProvider, serviceCollection);
        }

        private class WrappingServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _services;

            // Need full wrap for generics like IOptions
            public WrappingServiceProvider(IServiceProvider fallback, IServiceCollection replacedServices)
            {
                var services = new ServiceCollection();
                var manifest = fallback.GetRequiredService<IServiceManifest>();
                foreach (var service in manifest.Services) {
                    services.AddTransient(service, sp => fallback.GetService(service));
                }
                
                services.AddSingleton<IServiceManifest>(sp => new HostingManifest(services));
                services.Add(replacedServices);

                _services = services.BuildServiceProvider();
            }

            public object GetService(Type serviceType) {
                return _services.GetService(serviceType);
            }


            // Manifest exposes the fallback manifest in addition to ITypeActivator, IHostingEnvironment, and ILoggerFactory
            private class HostingManifest : IServiceManifest {
                public HostingManifest(IServiceCollection hostServices) {
                    Services = new Type[] {
                    typeof(IHostingEnvironment),
                    typeof(ILoggerFactory),
                    typeof(IHttpContextAccessor),
                    typeof(IApplicationLifetime)
                }.Concat(hostServices.Select(s => s.ServiceType)).Distinct();
                }

                public IEnumerable<Type> Services { get; private set; }
            }
        }
    }
}