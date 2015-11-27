using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Builders.Models;
using YesSql.Core.Indexes;
using YesSql.Core.Services;
using System.Data.SqlClient;
using System.Data;
using Orchard.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Orchard.Environment.Extensions;
using Orchard.FileSystem.AppData;
using System.IO;
using YesSql.Storage.Sql;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IAppDataFolderRoot _appDataFolderRoot;
        private readonly IServiceCollection _applicationServices;

        public ShellContainerFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IAppDataFolderRoot appDataFolderRoot,
            IServiceCollection applicationServices)
        {
            _applicationServices = applicationServices;
            _serviceProvider = serviceProvider;
            _appDataFolderRoot = appDataFolderRoot;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ShellContainerFactory>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            var featureByType = blueprint.Dependencies.ToDictionary(x => x.Type, x => x.Feature);
            IServiceCollection tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

            tenantServiceCollection.AddInstance(settings);
            tenantServiceCollection.AddInstance(blueprint.Descriptor);
            tenantServiceCollection.AddInstance(blueprint);

            // Sure this is right?
            tenantServiceCollection.AddInstance(_loggerFactory);

            foreach (var dependency in blueprint.Dependencies
                .Where(t => !typeof(IModule).IsAssignableFrom(t.Type)))
            {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf)))
                {
                    _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType))
                    {
                        tenantServiceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else
                    {
                        tenantServiceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            // Configure event handlers, they are not part of the blueprint, so they have
            // to be added manually. Or need to create a module for this.
            tenantServiceCollection.AddScoped<IEventBus, DefaultOrchardEventBus>();
            tenantServiceCollection.AddSingleton<IEventBusState, EventBusState>();

            // Configuring data access
            var indexes = blueprint
            .Dependencies
            .Where(x => typeof(IIndexProvider).IsAssignableFrom(x.Type))
            .Select(x => x.Type).ToArray();

            if (settings.DatabaseProvider != null)
            {
                var store = new Store(cfg =>
                    {
                        // @"Data Source =.; Initial Catalog = test1; User Id=sa;Password=demo123!"

                        IConnectionFactory connectionFactory = null;

                        switch (settings.DatabaseProvider)
                        {
                            case "SqlConnection":
                                connectionFactory = new DbConnectionFactory<SqlConnection>(settings.ConnectionString);
                                break;
                            //case "SqliteConnection":
                            //    connectionFactory = new DbConnectionFactory<SqliteConnection>(settings.ConnectionString);
                            //    break;
                            default:
                                throw new ArgumentException("Unkown database provider: " + settings.DatabaseProvider);
                        }

                        var sqlFactory = new SqlDocumentStorageFactory(connectionFactory); ;

                        cfg.ConnectionFactory = connectionFactory;
                        cfg.DocumentStorageFactory = sqlFactory;
                        cfg.IsolationLevel = sqlFactory.IsolationLevel = IsolationLevel.ReadUncommitted;

                        if (!String.IsNullOrWhiteSpace(settings.TablePrefix))
                        {
                            cfg.TablePrefix = sqlFactory.TablePrefix = settings.TablePrefix + "_";
                        }

                        //cfg.RunDefaultMigration();
                    }
                );

                var idGenerator = new LinearBlockIdGenerator(store.Configuration.ConnectionFactory, 20, "contentitem", store.Configuration.TablePrefix);

                store.RegisterIndexes(indexes);

                tenantServiceCollection.AddInstance<IStore>(store);
                tenantServiceCollection.AddInstance<LinearBlockIdGenerator>(idGenerator);

                tenantServiceCollection.AddScoped<ISession>(serviceProvider =>
                    store.CreateSession()
                );
            }

            tenantServiceCollection.AddInstance<ITypeFeatureProvider>(new TypeFeatureProvider(featureByType));

            IServiceCollection moduleServiceCollection =
                _serviceProvider.CreateChildContainer(_applicationServices);

            foreach (var dependency in blueprint.Dependencies
                .Where(t => typeof(IModule).IsAssignableFrom(t.Type)))
            {
                moduleServiceCollection.AddScoped(typeof(IModule), dependency.Type);
            }

            var moduleServiceProvider = moduleServiceCollection.BuildServiceProvider();

            // Let any module add custom service descriptors to the tenant
            foreach (var service in moduleServiceProvider.GetServices<IModule>())
            {
                service.Configure(tenantServiceCollection);
            }

            // Register event handlers on the event bus
            var eventHandlers = tenantServiceCollection
                .Select(x => x.ImplementationType)
                .Where(t => t != null && typeof(IEventHandler).IsAssignableFrom(t) && t.GetTypeInfo().IsClass)
                .ToArray();

            foreach (var handlerClass in eventHandlers)
            {
                tenantServiceCollection.AddScoped(handlerClass);

                // Register dynamic proxies to intercept direct calls if an IEventHandler is resolved, dispatching the call to
                // the event bus.

                foreach (var i in handlerClass.GetInterfaces().Where(t => typeof(IEventHandler).IsAssignableFrom(t)))
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
            var eventBusState = shellServiceProvider.GetService<IEventBusState>();

            // Register any IEventHandler method in the event bus
            foreach (var handlerClass in eventHandlers)
            {
                foreach (var handlerInterface in handlerClass.GetInterfaces().Where(x => typeof(IEventHandler).IsAssignableFrom(x)))
                {
                    foreach (var interfaceMethod in handlerInterface.GetMethods())
                    {
                        //var classMethod = handlerClass.GetMethods().Where(x => x.Name == interfaceMethod.Name && x.GetParameters().Length == interfaceMethod.GetParameters().Length).FirstOrDefault();
                        Func<IServiceProvider, IDictionary<string, object>, Task> d = (sp, parameters) => DefaultOrchardEventBus.Invoke(sp, parameters, interfaceMethod, handlerClass);
                        eventBusState.Add(handlerInterface.Name + "." + interfaceMethod.Name, d);
                    }
                }
            }

            return shellServiceProvider;
        }
    }
}