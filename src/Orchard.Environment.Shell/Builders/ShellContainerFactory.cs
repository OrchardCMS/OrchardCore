using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Builders.Models;
using YesSql.Core.Indexes;
using YesSql.Core.Services;
using System.Data.SqlClient;
using YesSql.Core.Storage.InMemory;
using System.Data;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Environment.Shell.Builders {
    public class ShellContainerFactory : IShellContainerFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public ShellContainerFactory(IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ShellContainerFactory>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);

            // Sure this is right?
            serviceCollection.AddInstance(_loggerFactory);

            IServiceCollection moduleServiceCollection = new ServiceCollection();
            foreach (var dependency in blueprint.Dependencies
                .Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {

                moduleServiceCollection.AddScoped(typeof(IModule), dependency.Type);
            }

            var moduleServiceProvider = moduleServiceCollection.BuildShellServiceProviderWithHost(_serviceProvider);
            foreach (var service in moduleServiceProvider.GetServices<IModule>()) {
                service.Configure(serviceCollection);
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

            // Configuring data access

            var indexes = blueprint
                .Dependencies
                .Where(x => typeof(IIndexProvider).IsAssignableFrom(x.Type))
                .Select(x => x.Type).ToArray();
            
            serviceCollection.AddSingleton<IStore>(serviceProvider =>
            {
                var store = new Store(cfg =>
                {
                    cfg.ConnectionFactory = new DbConnectionFactory<SqlConnection>(@"Data Source =.; Initial Catalog = test1; User Id=sa;Password=demo123!");
                    //cfg.ConnectionFactory = new DbConnectionFactory<SqliteConnection>(@"Data Source=" + dbFileName + ";Cache=Shared");
                    cfg.DocumentStorageFactory = new InMemoryDocumentStorageFactory();
                    cfg.IsolationLevel = IsolationLevel.ReadUncommitted;
                    cfg.RunDefaultMigration();
                });

                store.RegisterIndexes(indexes);
                return store;

            });

            serviceCollection.AddScoped<ISession>(serviceProvider => {
                var store = serviceProvider.GetRequiredService<IStore>();
                return store.CreateSession();
            });

            return serviceCollection.BuildShellServiceProviderWithHost(_serviceProvider);
        }
    }
}