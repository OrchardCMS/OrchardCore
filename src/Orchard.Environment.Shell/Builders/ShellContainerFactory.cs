#define SQL

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Events;
using YesSql.Core.Indexes;
using YesSql.Core.Services;
using YesSql.Storage.Sql;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceCollection _applicationServices;

        public ShellContainerFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ILogger<ShellContainerFactory> logger,
            IServiceCollection applicationServices)
        {
            _applicationServices = applicationServices;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public void AddCoreServices(IServiceCollection services)
        {
            services.AddScoped<IShellStateUpdater, ShellStateUpdater>();
            services.AddScoped<IShellStateManager, ShellStateManager>();
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

            foreach (var dependency in blueprint.Dependencies)
            {
                foreach (var interfaceType in dependency.Type.GetInterfaces())
                {
                    // GetInterfaces returns the full hierarchy of interfaces
                    if (interfaceType == typeof(ISingletonDependency) ||
                        interfaceType == typeof(ITransientDependency) ||
                        interfaceType == typeof(IDependency) ||
                        !typeof(IDependency).IsAssignableFrom(interfaceType))
                    {
                        continue;
                    }

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);
                    }

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else if (typeof(IDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            // Register components
            foreach (var dependency in blueprint.Dependencies)
            {
                var serviceComponentAttribute = dependency.Type.GetTypeInfo().GetCustomAttribute<ServiceScopeAttribute>();
                if (serviceComponentAttribute != null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, serviceComponentAttribute.ServiceType);
                    }

                    serviceComponentAttribute.Register(tenantServiceCollection, dependency.Type);
                }
            }

            // Configure event handlers, they are not part of the blueprint, so they have
            // to be added manually. Or need to create a module for this.
            tenantServiceCollection.AddScoped<IEventBus, DefaultOrchardEventBus>();
            tenantServiceCollection.AddSingleton<IEventBusState, EventBusState>();

            //// Apply custom options for the tenant
            //var options = blueprint
            //.Dependencies
            //.Where(x => typeof(IConfigure).IsAssignableFrom(x.Type))
            //.Select(x => x.Type).ToArray();

            //// TODO: Group all options by type and reuse the same configuration object
            //// such that multiple feature can update the same configuration object.

            //foreach (var type in options)
            //{
            //    var optionType = type
            //        .GetInterfaces()
            //        .Where(x => typeof(IConfigure).IsAssignableFrom(x))
            //        .FirstOrDefault()
            //        .GetGenericArguments()
            //        .FirstOrDefault();

            //    if(optionType == null)
            //    {
            //        // Ignore non-generic implementation
            //        continue;
            //    }

            //    var optionObject = Activator.CreateInstance(optionType);
            //    var configureMethod = type.GetMethod("Configure");
            //    var optionHost = Activator.CreateInstance(type);
            //    configureMethod.Invoke(optionHost, new[] { optionObject });
            //    tenantServiceCollection.ConfigureOptions(optionObject);
            //}

            // Execute IStartup registrations

            // TODO: Use StartupLoader in RTM and then don't need to register the classes anymore then

            IServiceCollection moduleServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            foreach (var dependency in blueprint.Dependencies.Where(t => typeof(IStartup).IsAssignableFrom(t.Type)))
            {
                moduleServiceCollection.AddSingleton(typeof(IStartup), dependency.Type);
                tenantServiceCollection.AddSingleton(typeof(IStartup), dependency.Type);
            }

            // Make shell settings available to the modules
            moduleServiceCollection.AddSingleton(settings);

            var moduleServiceProvider = moduleServiceCollection.BuildServiceProvider();

            // Let any module add custom service descriptors to the tenant
            foreach (var service in moduleServiceProvider.GetServices<IStartup>())
            {
                service.ConfigureServices(tenantServiceCollection);
            }

            (moduleServiceProvider as IDisposable).Dispose();

            // Configuring data access

            var indexes = tenantServiceCollection
                .Select(x => x.ImplementationType)
                .Where(t => t != null && typeof(IIndexProvider).IsAssignableFrom(t) && t.GetTypeInfo().IsClass)
                .Distinct()
                .ToArray();


            if (settings.DatabaseProvider != null)
            {
                IConnectionFactory connectionFactory = null;

                switch (settings.DatabaseProvider)
                {
                    case "SqlConnection":
                        connectionFactory = new DbConnectionFactory<SqlConnection>(settings.ConnectionString);
                        break;
                    case "SqliteConnection":
                        connectionFactory = new DbConnectionFactory<SqliteConnection>(settings.ConnectionString);
                        break;
                    default:
                        throw new ArgumentException("Unknown database provider: " + settings.DatabaseProvider);
                }

                var configuration = new Configuration
                {
                    ConnectionFactory = connectionFactory,
                    IsolationLevel = IsolationLevel.ReadUncommitted
                };

                if (!string.IsNullOrWhiteSpace(settings.TablePrefix))
                {
                    configuration.TablePrefix = settings.TablePrefix + "_";
                }

#if SQL
                var sqlFactory = new SqlDocumentStorageFactory();
                if (!string.IsNullOrWhiteSpace(settings.TablePrefix))
                {
                    sqlFactory.TablePrefix = settings.TablePrefix + "_";
                }
                configuration.DocumentStorageFactory = sqlFactory;
#else
                        var storageFactory = new LightningDocumentStorageFactory(Path.Combine(_appDataFolderRoot.RootFolder, "Sites", settings.Name, "Documents"));
                        cfg.DocumentStorageFactory = storageFactory;
#endif

                var store = new Store(configuration);
                var idGenerator = new LinearBlockIdGenerator(store.Configuration.ConnectionFactory, 20, store.Configuration.TablePrefix);

                store.RegisterIndexes(indexes);

                tenantServiceCollection.AddSingleton<IStore>(store);
                tenantServiceCollection.AddSingleton<LinearBlockIdGenerator>(idGenerator);

                tenantServiceCollection.AddScoped<ISession>(serviceProvider => store.CreateSession());
            }

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