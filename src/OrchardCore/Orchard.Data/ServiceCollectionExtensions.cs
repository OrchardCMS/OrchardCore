using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Npgsql;
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
            services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

            // Adding supported databases
            services.TryAddDataProvider(name: "Sql Server", value: "SqlConnection", hasConnectionString: true);
            services.TryAddDataProvider(name: "Sqlite", value: "Sqlite", hasConnectionString: false);
			services.TryAddDataProvider(name: "MySql", value: "MySql", hasConnectionString: true);
			services.TryAddDataProvider(name: "Postgres", value: "Postgres", hasConnectionString: true);

			// Configuring data access

			services.AddSingleton<IStore>(sp =>
            {
                var shellSettings = sp.GetService<ShellSettings>();
                var hostingEnvironment = sp.GetService<IHostingEnvironment>();

                if (shellSettings.DatabaseProvider == null)
                {
                    return null;
                }

                var shellOptions = sp.GetService<IOptions<ShellOptions>>();

                IConnectionFactory connectionFactory = null;
                var isolationLevel = IsolationLevel.Unspecified;

                switch (shellSettings.DatabaseProvider)
                {
                    case "SqlConnection":
                        connectionFactory = new DbConnectionFactory<SqlConnection>(shellSettings.ConnectionString);
                        isolationLevel = IsolationLevel.ReadUncommitted;
                        break;
                    case "Sqlite":
                        var option = shellOptions.Value;
                        var databaseFolder = Path.Combine(hostingEnvironment.ContentRootPath, option.ShellsRootContainerName, option.ShellsContainerName, shellSettings.Name);
                        var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                        Directory.CreateDirectory(databaseFolder);
                        connectionFactory = new DbConnectionFactory<SqliteConnection>($"Data Source={databaseFile};Cache=Shared");
                        isolationLevel = IsolationLevel.ReadUncommitted;
						break;
					case "MySql":
						connectionFactory = new DbConnectionFactory<MySqlConnection>(shellSettings.ConnectionString);
						isolationLevel = IsolationLevel.ReadUncommitted;
						break;
					case "Postgres":
						connectionFactory = new DbConnectionFactory<NpgsqlConnection>(shellSettings.ConnectionString);
						isolationLevel = IsolationLevel.ReadUncommitted;
						break;
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