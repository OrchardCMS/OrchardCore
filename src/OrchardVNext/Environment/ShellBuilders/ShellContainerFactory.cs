using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.Runtime;
using OrchardVNext.Data;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders.Models;
using OrchardVNext.Mvc;
using OrchardVNext.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardVNext.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory {
        private readonly IServiceProvider _serviceProvider;

        public ShellContainerFactory(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IOrchardShell, DefaultOrchardShell>();
            serviceCollection.AddScoped<IRouteBuilder, DefaultShellRouteBuilder>();
            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);
            
            serviceCollection.AddMvc();

            serviceCollection.Configure<RazorViewEngineOptions>(options => {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });

            var p = _serviceProvider.GetService<IOrchardLibraryManager>();
            serviceCollection.AddInstance<IAssemblyProvider>(new DefaultAssemblyProviderTest(p, _serviceProvider, _serviceProvider.GetService<IAssemblyLoaderContainer>()));

            foreach (var dependency in blueprint.Dependencies) {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                    Logger.Debug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof (ITransientDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            return BuildFallbackServiceProvider(
                            serviceCollection,
                            _serviceProvider);
        }


        private static IServiceProvider BuildFallbackServiceProvider(IEnumerable<IServiceDescriptor> services, 
            IServiceProvider fallback) {
            var sc = HostingServices.Create(fallback);
            sc.Add(services);

            // Build the manifest
            var manifestTypes = services.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                    && t.ServiceType != typeof(IServiceManifest)
                    && t.ServiceType != typeof(IServiceProvider))
                    .Select(t => t.ServiceType).Distinct();
            sc.AddInstance<IServiceManifest>(new ServiceManifest(manifestTypes, fallback.GetRequiredService<IServiceManifest>()));
            return sc.BuildServiceProvider();
        }

        private class ServiceManifest : IServiceManifest {
            public ServiceManifest(IEnumerable<Type> services, IServiceManifest fallback = null) {
                Services = services;
                if (fallback != null) {
                    Services = Services.Concat(fallback.Services).Distinct();
                }
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}