using System;
using System.Buffers;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Data.YesSql;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for <see cref="OrchardCoreBuilder"/> to add database access functionality.
    /// </summary>
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level data access services.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddDataAccess(this OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddSingleton<IShellRemovingHandler, ShellDbTablesRemovingHandler>();

            builder.ConfigureServices(services =>
            {
                services.AddScoped<IDbConnectionValidator, DbConnectionValidator>();
                services.AddScoped<IDataMigrationManager, DataMigrationManager>();
                services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

                services.AddTransient<ITableNameConventionFactory, TableNameConventionFactory>();
                services.AddTransient<IConfigureOptions<SqliteOptions>, SqliteOptionsConfiguration>();

                // Adding supported databases
                services.TryAddDataProvider(name: "Sql Server", value: DatabaseProviderValue.SqlConnection, hasConnectionString: true, sampleConnectionString: "Server=localhost;Database=Orchard;User Id=username;Password=password", hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Sqlite", value: DatabaseProviderValue.Sqlite, hasConnectionString: false, hasTablePrefix: false, isDefault: true);
                services.TryAddDataProvider(name: "MySql", value: DatabaseProviderValue.MySql, hasConnectionString: true, sampleConnectionString: "Server=localhost;Database=Orchard;Uid=username;Pwd=password", hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Postgres", value: DatabaseProviderValue.Postgres, hasConnectionString: true, sampleConnectionString: "Server=localhost;Port=5432;Database=Orchard;User Id=username;Password=password", hasTablePrefix: true, isDefault: false);

                // Configuring data access
                services.AddSingleton(sp =>
                {
                    var shellSettings = sp.GetService<ShellSettings>();

                    // Before the setup, a 'DatabaseProvider' may be configured without a required 'ConnectionString'.
                    if (shellSettings.IsUninitialized() || shellSettings["DatabaseProvider"] is null)
                    {
                        return null;
                    }

                    var yesSqlOptions = sp.GetService<IOptions<YesSqlOptions>>().Value;
                    var databaseTableOptions = shellSettings.GetDatabaseTableOptions();
                    var storeConfiguration = GetStoreConfiguration(sp, yesSqlOptions, databaseTableOptions);

                    switch (shellSettings["DatabaseProvider"])
                    {
                        case DatabaseProviderValue.SqlConnection:
                            storeConfiguration
                                .UseSqlServer(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted, shellSettings["Schema"])
                                .UseBlockIdGenerator();
                            break;
                        case DatabaseProviderValue.Sqlite:
                            var shellOptions = sp.GetService<IOptions<ShellOptions>>().Value;
                            var sqliteOptions = sp.GetService<IOptions<SqliteOptions>>().Value;

                            var databaseFolder = SqliteHelper.GetDatabaseFolder(shellOptions, shellSettings.Name);
                            Directory.CreateDirectory(databaseFolder);

                            var connectionString = SqliteHelper.GetConnectionString(sqliteOptions, databaseFolder);
                            storeConfiguration
                                .UseSqLite(connectionString, IsolationLevel.ReadUncommitted)
                                .UseDefaultIdGenerator();
                            break;
                        case DatabaseProviderValue.MySql:
                            storeConfiguration
                                .UseMySql(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted, shellSettings["Schema"])
                                .UseBlockIdGenerator();
                            break;
                        case DatabaseProviderValue.Postgres:
                            storeConfiguration
                                .UsePostgreSql(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted, shellSettings["Schema"])
                                .UseBlockIdGenerator();
                            break;
                        default:
                            throw new ArgumentException("Unknown database provider: " + shellSettings["DatabaseProvider"]);
                    }

                    if (!String.IsNullOrWhiteSpace(shellSettings["TablePrefix"]))
                    {
                        var tablePrefix = shellSettings["TablePrefix"].Trim() + databaseTableOptions.TableNameSeparator;

                        storeConfiguration = storeConfiguration.SetTablePrefix(tablePrefix);
                    }

                    var store = StoreFactory.Create(storeConfiguration);

                    var indexes = sp.GetServices<IIndexProvider>();

                    store.RegisterIndexes(indexes);

                    return store;
                });

                services.Initialize(async sp =>
                {
                    var store = sp.GetService<IStore>();
                    if (store == null)
                    {
                        return;
                    }

                    await store.InitializeAsync();

                    var storeCollectionOptions = sp.GetService<IOptions<StoreCollectionOptions>>().Value;
                    foreach (var collection in storeCollectionOptions.Collections)
                    {
                        await store.InitializeCollectionAsync(collection);
                    }
                });

                services.AddScoped(sp =>
                {
                    var store = sp.GetService<IStore>();

                    if (store == null)
                    {
                        return null;
                    }

                    var session = store.CreateSession();

                    var scopedServices = sp.GetServices<IScopedIndexProvider>();

                    session.RegisterIndexes(scopedServices.ToArray());

                    ShellScope.Current
                        .RegisterBeforeDispose(scope =>
                        {
                            return scope.ServiceProvider
                                .GetRequiredService<IDocumentStore>()
                                .CommitAsync();
                        })
                        .AddExceptionHandler((scope, e) =>
                        {
                            return scope.ServiceProvider
                                .GetRequiredService<IDocumentStore>()
                                .CancelAsync();
                        });

                    return session;
                });

                services.AddScoped<IDocumentStore, DocumentStore>();
                services.AddSingleton<IFileDocumentStore, FileDocumentStore>();
                services.AddTransient<IDbConnectionAccessor, DbConnectionAccessor>();
            });

            return builder;
        }

        private static IConfiguration GetStoreConfiguration(IServiceProvider sp, YesSqlOptions yesSqlOptions, DatabaseTableOptions databaseTableOptions)
        {
            var tableNameFactory = sp.GetRequiredService<ITableNameConventionFactory>();

            var storeConfiguration = new YesSql.Configuration
            {
                CommandsPageSize = yesSqlOptions.CommandsPageSize,
                QueryGatingEnabled = yesSqlOptions.QueryGatingEnabled,
                ContentSerializer = new PoolingJsonContentSerializer(sp.GetService<ArrayPool<char>>()),
                TableNameConvention = tableNameFactory.Create(databaseTableOptions),
                IdentityColumnSize = Enum.Parse<IdentityColumnSize>(databaseTableOptions.IdentityColumnSize),
            };

            if (yesSqlOptions.IdGenerator != null)
            {
                storeConfiguration.IdGenerator = yesSqlOptions.IdGenerator;
            }

            if (yesSqlOptions.IdentifierAccessorFactory != null)
            {
                storeConfiguration.IdentifierAccessorFactory = yesSqlOptions.IdentifierAccessorFactory;
            }

            if (yesSqlOptions.VersionAccessorFactory != null)
            {
                storeConfiguration.VersionAccessorFactory = yesSqlOptions.VersionAccessorFactory;
            }

            if (yesSqlOptions.ContentSerializer != null)
            {
                storeConfiguration.ContentSerializer = yesSqlOptions.ContentSerializer;
            }

            return storeConfiguration;
        }
    }
}
