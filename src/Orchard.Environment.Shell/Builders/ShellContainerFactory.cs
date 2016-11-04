using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Events;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceCollection _applicationServices;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ShellContainerFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ILogger<ShellContainerFactory> logger,
            IServiceCollection applicationServices,
            IHostingEnvironment hostingEnvironment)
        {
            _applicationServices = applicationServices;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public void AddCoreServices(IServiceCollection services)
        {
            services.TryAddScoped<IShellStateUpdater, ShellStateUpdater>();
            services.TryAddScoped<IShellStateManager, NullShellStateManager>();
            services.AddScoped<ShellStateCoordinator>();
            services.AddScoped<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<ShellStateCoordinator>());
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            IServiceCollection tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            tenantServiceCollection.AddSingleton(settings);
            tenantServiceCollection.AddSingleton(blueprint.Descriptor);
            tenantServiceCollection.AddSingleton(blueprint);

            AddCoreServices(tenantServiceCollection);

            // Sure this is right?
            tenantServiceCollection.AddSingleton(_loggerFactory);
            
            // Configure event handlers, they are not part of the blueprint, so they have
            // to be added manually. Or need to create a module for this.
            tenantServiceCollection.AddScoped<IEventBus, DefaultOrchardEventBus>();
            tenantServiceCollection.AddSingleton<IEventBusState, EventBusState>();

            // Execute IStartup registrations

            // TODO: Use StartupLoader in RTM and then don't need to register the classes anymore then

            IServiceCollection moduleServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            foreach (var dependency in blueprint.Dependencies.Where(t => typeof(Microsoft.AspNetCore.Mvc.Modules.IStartup).IsAssignableFrom(t.Type)))
            {
                moduleServiceCollection.AddSingleton(typeof(Microsoft.AspNetCore.Mvc.Modules.IStartup), dependency.Type);
                tenantServiceCollection.AddSingleton(typeof(Microsoft.AspNetCore.Mvc.Modules.IStartup), dependency.Type);
            }

            // Add a default configuration if none has been provided
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            moduleServiceCollection.TryAddSingleton(configuration);
            tenantServiceCollection.TryAddSingleton(configuration);

            // Make shell settings available to the modules
            moduleServiceCollection.AddSingleton(settings);

            var moduleServiceProvider = moduleServiceCollection.BuildServiceProvider();

            // Let any module add custom service descriptors to the tenant
            foreach (var service in moduleServiceProvider.GetServices<Microsoft.AspNetCore.Mvc.Modules.IStartup>())
            {
                service.ConfigureServices(tenantServiceCollection);
            }

            (moduleServiceProvider as IDisposable).Dispose();

            // add already instanciated services like DefaultOrchardHost
            var applicationServiceDescriptors = _applicationServices.Where(x => x.Lifetime == ServiceLifetime.Singleton);

            // Register event handlers on the event bus
            var eventHandlers = tenantServiceCollection
                .Union(applicationServiceDescriptors)
                .Select(x => x.ImplementationType)
                .Distinct()
                .Where(t => t != null && typeof(IEventHandler).IsAssignableFrom(t) && t.GetTypeInfo().IsClass)
                .ToArray();

            foreach (var handlerClass in eventHandlers)
            {
                // Register dynamic proxies to intercept direct calls if an IEventHandler is resolved, dispatching the call to
                // the event bus.

                foreach (var i in handlerClass.GetInterfaces().Where(t => t != typeof(IEventHandler) && typeof(IEventHandler).IsAssignableFrom(t)))
                {
                    tenantServiceCollection.AddScoped(i, serviceProvider =>
                    {
                        var proxy = DefaultOrchardEventBus.CreateProxy(i);
                        proxy.EventBus = serviceProvider.GetService<IEventBus>();
                        return proxy;
                    });
                }
            }

            var shellServiceProvider = tenantServiceCollection.BuildServiceProvider();

            using (var scope = shellServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var eventBusState = scope.ServiceProvider.GetService<IEventBusState>();

                // Register any IEventHandler method in the event bus
                foreach (var handlerClass in eventHandlers)
                {
                    foreach (var handlerInterface in handlerClass.GetInterfaces().Where(x => typeof(IEventHandler).IsAssignableFrom(x) && typeof(IEventHandler) != x))
                    {
                        foreach (var interfaceMethod in handlerInterface.GetMethods())
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"{handlerClass.Name}/{handlerInterface.Name}.{interfaceMethod.Name}");
                            }

                            //var classMethod = handlerClass.GetMethods().Where(x => x.Name == interfaceMethod.Name && x.GetParameters().Length == interfaceMethod.GetParameters().Length).FirstOrDefault();
                            Func<IServiceProvider, IDictionary<string, object>, Task> d = (sp, parameters) => DefaultOrchardEventBus.Invoke(sp, parameters, interfaceMethod, handlerClass);
                            var messageName = $"{handlerInterface.Name}.{interfaceMethod.Name}";
                            var className = handlerClass.FullName;
                            eventBusState.Add(messageName, d);
                        }
                    }
                }
            }

            return shellServiceProvider;
        }
    }
}