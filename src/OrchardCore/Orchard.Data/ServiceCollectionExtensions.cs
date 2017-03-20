using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using OrchardCore.Modules;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Npgsql;
using Orchard.Data.Migration;
using OrchardCore.Tenant;
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
            services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

            // Adding supported databases
            services.TryAddDataProvider(name: "Sql Server", value: "SqlConnection", hasConnectionString: true);
            services.TryAddDataProvider(name: "Sqlite", value: "Sqlite", hasConnectionString: false);
			services.TryAddDataProvider(name: "MySql", value: "MySql", hasConnectionString: true);
			services.TryAddDataProvider(name: "Postgres", value: "Postgres", hasConnectionString: true);

			// Configuring data access

			services.AddSingleton<IStore>(sp =>
            {
                var tenantSettings = sp.GetService<TenantSettings>();
                var hostingEnvironment = sp.GetService<IHostingEnvironment>();

                if (tenantSettings.DatabaseProvider == null)
                {
                    return null;
                }

                var tenantOptions = sp.GetService<IOptions<TenantOptions>>();

                IConnectionFactory connectionFactory = null;
                var isolationLevel = IsolationLevel.Unspecified;

                switch (tenantSettings.DatabaseProvider)
                {
                    case "SqlConnection":
                        connectionFactory = new DbConnectionFactory<SqlConnection>(tenantSettings.ConnectionString);
                        isolationLevel = IsolationLevel.ReadUncommitted;
                        break;
                    case "Sqlite":
                        var option = tenantOptions.Value;
                        var databaseFolder = Path.Combine(hostingEnvironment.ContentRootPath, option.TenantsRootContainerName, option.TenantsContainerName, tenantSettings.Name);
                        var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                        Directory.CreateDirectory(databaseFolder);
                        connectionFactory = new DbConnectionFactory<SqliteConnection>($"Data Source={databaseFile};Cache=Shared");
                        isolationLevel = IsolationLevel.ReadUncommitted;
						break;
					case "MySql":
						connectionFactory = new DbConnectionFactory<MySqlConnection>(tenantSettings.ConnectionString);
						isolationLevel = IsolationLevel.ReadUncommitted;
						break;
					case "Postgres":
						connectionFactory = new DbConnectionFactory<NpgsqlConnection>(tenantSettings.ConnectionString);
						isolationLevel = IsolationLevel.ReadUncommitted;
						break;
					default:
                        throw new ArgumentException("Unknown database provider: " + tenantSettings.DatabaseProvider);
                }

                var storeConfiguration = new Configuration
                {
                    ConnectionFactory = connectionFactory,
                    IsolationLevel = isolationLevel
                };

                if (!string.IsNullOrWhiteSpace(tenantSettings.TablePrefix))
                {
                    storeConfiguration.TablePrefix = tenantSettings.TablePrefix + "_";
                }

                var sqlFactory = new SqlDocumentStorageFactory();
                if (!string.IsNullOrWhiteSpace(tenantSettings.TablePrefix))
                {
                    sqlFactory.TablePrefix = tenantSettings.TablePrefix + "_";
                }
                storeConfiguration.DocumentStorageFactory = sqlFactory;

                var store = new Store(storeConfiguration);
                var indexes = sp.GetServices<IIndexProvider>();
                store.RegisterIndexes(indexes.ToArray());
                return store;
            });

            services.AddScoped<ISession>(sp =>
            {
                var store = sp.GetService<IStore>();

                if (store == null)
                {
                    return null;
                }

                return store.CreateSession();
            });

            return services;
        }
    }
}