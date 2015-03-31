using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders.Models;
using OrchardVNext.Mvc;
using OrchardVNext.Routing;
using System;
using System.Linq;
using System.Reflection;
using OrchardVNext.Data.EF;

namespace OrchardVNext.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ShellContainerFactory(IServiceProvider serviceProvider)
        {
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

            serviceCollection.AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<DataContext>();

            serviceCollection.AddMvc();

            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });

            var p = _serviceProvider.GetService<IOrchardLibraryManager>();
            serviceCollection.AddInstance<IAssemblyProvider>(new DefaultAssemblyProviderTest(p, _serviceProvider, _serviceProvider.GetService<IAssemblyLoaderContainer>()));

            foreach (var dependency in blueprint.Dependencies)
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

            var sc = HostingServices.Create(_serviceProvider);
            sc.Add(serviceCollection);

            return sc.BuildServiceProvider();
        }
    }
}