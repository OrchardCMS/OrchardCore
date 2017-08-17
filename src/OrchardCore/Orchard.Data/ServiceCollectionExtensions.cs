using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orchard.Data.Migration;
using Orchard.Environment.Shell;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();
            services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

            // Adding supported databases
            TryAddSupportedDatabaseProviders(services);

			// Configuring data access
			services.AddSingleton<IStore>(sp =>
            {
                var shellSettings = sp.GetService<ShellSettings>();

                if (shellSettings.DatabaseProvider == null)
                {
                    return null;
                }
                
                var storeConfiguration = new Configuration();
                var databaseProvider = shellSettings.DatabaseProvider;

                if (databaseProvider == DatabaseProviderName.SqlServer)
                {
                    storeConfiguration.UseSqlServer(shellSettings.ConnectionString);
                }
                else if (databaseProvider == DatabaseProviderName.Sqlite)
                {
                    var shellOptions = sp.GetService<IOptions<ShellOptions>>();
                    var option = shellOptions.Value;
                    var hostingEnvironment = sp.GetService<IHostingEnvironment>();
                    var databaseFolder = Path.Combine(hostingEnvironment.ContentRootPath, option.ShellsRootContainerName, option.ShellsContainerName, shellSettings.Name);
                    var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                    Directory.CreateDirectory(databaseFolder);
                    storeConfiguration.UseSqLite($"Data Source={databaseFile};Cache=Shared");
                }
                else if (databaseProvider == DatabaseProviderName.MySQL)
                {
                    storeConfiguration.UseMySql(shellSettings.ConnectionString);
                }
                else if (databaseProvider == DatabaseProviderName.Postgres)
                {
                    storeConfiguration.UsePostgreSql(shellSettings.ConnectionString);
                }
                else
                {
                    throw new ArgumentException($"Unknown database provider: {shellSettings.DatabaseProvider}");
                }

                if (!string.IsNullOrWhiteSpace(shellSettings.TablePrefix))
                {
                    storeConfiguration.TablePrefix = shellSettings.TablePrefix + "_";
                }

                var store = new Store(storeConfiguration);
                var indexes = sp.GetServices<IIndexProvider>();
                store.RegisterIndexes(indexes.ToArray());
                return store;
            });

            services.AddScoped(sp =>
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

        private static void TryAddSupportedDatabaseProviders(IServiceCollection services)
        {
            services.TryAddDataProvider(DatabaseProviderName.SqlServer, DatabaseProviderName.SqlServer);
            services.TryAddDataProvider(DatabaseProviderName.Sqlite, DatabaseProviderName.Sqlite, hasConnectionString: false);
            services.TryAddDataProvider(DatabaseProviderName.MySQL, DatabaseProviderName.MySQL);
            services.TryAddDataProvider(DatabaseProviderName.Postgres, DatabaseProviderName.Postgres);
        }
    }
}