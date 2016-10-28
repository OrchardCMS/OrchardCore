using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Environment.Shell;
using YesSql.Core.Indexes;
using YesSql.Core.Services;
using YesSql.Storage.Sql;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();
            services.AddScoped<AutomaticDataMigrations>();

            // Adding supported databases

            services.AddSingleton(new DatabaseProvider
            {
                Name = "Sql Server",
                Value = "SqlConnection",
                HasConnectionString = true
            });

            services.AddSingleton(new DatabaseProvider
            {
                Name = "Sql Lite",
                Value = "Sqlite",
                HasConnectionString = false
            });

            // Configuring data access

            services.AddSingleton<IStore>(sp =>
            {
                var shellSettings = sp.GetService<ShellSettings>();

                if (shellSettings.DatabaseProvider == null)
                {
                    return null;
                }

                IConnectionFactory connectionFactory = null;
                var isolationLevel = IsolationLevel.Unspecified;

                switch (shellSettings.DatabaseProvider)
                {
                    case "SqlConnection":
                        connectionFactory = new DbConnectionFactory<SqlConnection>(shellSettings.ConnectionString);
                        isolationLevel = IsolationLevel.ReadUncommitted;
                        break;
                    //case "Sqlite":
                    //    var databaseFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "App_Data", "Sites", shellSettings.Name);
                    //    var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                    //    Directory.CreateDirectory(databaseFolder);
                    //    connectionFactory = new DbConnectionFactory<SqliteConnection>($"Data Source={databaseFile};Cache=Shared", false);
                    //    isolationLevel = IsolationLevel.ReadUncommitted;
                    //    break;
                    default:
                        throw new ArgumentException("Unknown database provider: " + shellSettings.DatabaseProvider);
                }

                var storeConfiguration = new Configuration
                {
                    ConnectionFactory = connectionFactory,
                    IsolationLevel = isolationLevel
                };

                if (!string.IsNullOrWhiteSpace(shellSettings.TablePrefix))
                {
                    storeConfiguration.TablePrefix = shellSettings.TablePrefix + "_";
                }

                var sqlFactory = new SqlDocumentStorageFactory();
                if (!string.IsNullOrWhiteSpace(shellSettings.TablePrefix))
                {
                    sqlFactory.TablePrefix = shellSettings.TablePrefix + "_";
                }
                storeConfiguration.DocumentStorageFactory = sqlFactory;

                var store = new Store(storeConfiguration);
                var indexes = sp.GetServices<IIndexProvider>();
                store.RegisterIndexes(indexes.ToArray());
                return store;
            });

            services.AddSingleton<LinearBlockIdGenerator>(sp =>
            {
                var store = sp.GetService<IStore>();
                var configuration = store.Configuration;
                var idGenerator = new LinearBlockIdGenerator(configuration.ConnectionFactory, 20, configuration.TablePrefix);
                return idGenerator;
            });

            services.AddScoped<ISession>(sp =>
            {
                var store = sp.GetService<IStore>();
                return store.CreateSession();
            });

            return services;
        }
    }
}