using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.ShellBuilders.Models;
using OrchardVNext.Routing;
using System;
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

            foreach (var dependency in blueprint.Dependencies) {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                    Console.WriteLine(dependency.Type);
                    serviceCollection.AddScoped(dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                }
            }

            return serviceCollection.BuildServiceProvider(_serviceProvider);
        }
    }
}