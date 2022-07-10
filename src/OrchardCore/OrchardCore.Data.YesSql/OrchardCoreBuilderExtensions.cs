using System;
using System.Buffers;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Data.YesSql.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
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
        /// Adds tenant level data access services.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddDataAccess(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IDbConnectionValidator, DbConnectionValidator>();
                services.AddScoped<IDataMigrationManager, DataMigrationManager>();
                services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

                services.AddOptions<StoreCollectionOptions>();
                services.AddTransient<IConfigureOptions<SqliteOptions>, SqliteOptionsConfiguration>();

                // Adding supported databases
                services.TryAddDataProvider(name: "Sql Server", value: DatabaseProviderName.SqlConnection, hasConnectionString: true, sampleConnectionString: "Server=localhost;Database=Orchard;User Id=username;Password=password", hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Sqlite", value: DatabaseProviderName.Sqlite, hasConnectionString: false, hasTablePrefix: false, isDefault: true);
                services.TryAddDataProvider(name: "MySql", value: DatabaseProviderName.MySql, hasConnectionString: true, sampleConnectionString: "Server=localhost;Database=Orchard;Uid=username;Pwd=password", hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Postgres", value: DatabaseProviderName.Postgres, hasConnectionString: true, sampleConnectionString: "Server=localhost;Port=5432;Database=Orchard;User Id=username;Password=password", hasTablePrefix: true, isDefault: false);

                // register ITableNameConvention
                services.AddScoped(sp =>
                {
                    var yesSqlOptions = sp.GetService<IOptions<YesSqlOptions>>().Value;

                    if (yesSqlOptions.TableNameConvention != null)
                    {
                        return yesSqlOptions.TableNameConvention;
                    }

                    return new YesSql.Configuration().TableNameConvention;
                });

                // Configuring data access
                services.AddSingleton(sp =>
                {
                    var shellSettings = sp.GetService<ShellSettings>();

                    // Before the setup, a 'DatabaseProvider' may be configured without a required 'ConnectionString'.
                    if (shellSettings.State == TenantState.Uninitialized || shellSettings["DatabaseProvider"] == null)
                    {
                        return null;
                    }

                    var yesSqlOptions = sp.GetService<IOptions<YesSqlOptions>>().Value;
                    var storeConfiguration = GetStoreConfiguration(sp, yesSqlOptions);

                    if (!Enum.TryParse(shellSettings["DatabaseProvider"], out DatabaseProviderName providerName))
                    {
                        throw new ArgumentException("Unknown database provider: " + shellSettings["DatabaseProvider"]);
                    }

                    switch (providerName)
                    {
                        case DatabaseProviderName.SqlConnection:
                            storeConfiguration
                                .UseSqlServer(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted)
                                .UseBlockIdGenerator();
                            break;
                        case DatabaseProviderName.Sqlite:
                            var shellOptions = sp.GetService<IOptions<ShellOptions>>();
                            var sqliteOptions = sp.GetService<IOptions<SqliteOptions>>()?.Value ?? new SqliteOptions();
                            var option = shellOptions.Value;
                            var databaseFolder = Path.Combine(option.ShellsApplicationDataPath, option.ShellsContainerName, shellSettings.Name);
                            var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                            var connectionStringBuilder = new SqliteConnectionStringBuilder
                            {
                                DataSource = databaseFile,
                                Cache = SqliteCacheMode.Shared,
                                Pooling = sqliteOptions.UseConnectionPooling
                            };

                            Directory.CreateDirectory(databaseFolder);
                            storeConfiguration
                                .UseSqLite(connectionStringBuilder.ToString(), IsolationLevel.ReadUncommitted)
                                .UseDefaultIdGenerator();
                            break;
                        case DatabaseProviderName.MySql:
                            storeConfiguration
                                .UseMySql(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted)
                                .UseBlockIdGenerator();
                            break;
                        case DatabaseProviderName.Postgres:
                            storeConfiguration
                                .UsePostgreSql(shellSettings["ConnectionString"], IsolationLevel.ReadUncommitted)
                                .UseBlockIdGenerator();
                            break;
                        default:
                            throw new ArgumentException("Unknown database provider: " + shellSettings["DatabaseProvider"]);
                    }

                    if (!String.IsNullOrEmpty(shellSettings["TablePrefix"]))
                    {
                        // For backward compatibility, if the TablePrefixSeparator isn't set, we use _ as the default value.
                        var seperator = shellSettings["TablePrefixSeparator"] ?? "_";

                        var tablePrefix = shellSettings["TablePrefix"].Trim() + (seperator ?? String.Empty);

                        storeConfiguration = storeConfiguration.SetTablePrefix(tablePrefix);
                    }

                    var store = StoreFactory.CreateAndInitializeAsync(storeConfiguration).GetAwaiter().GetResult();
                    var options = sp.GetService<IOptions<StoreCollectionOptions>>().Value;
                    foreach (var collection in options.Collections)
                    {
                        store.InitializeCollectionAsync(collection).GetAwaiter().GetResult();
                    }

                    var indexes = sp.GetServices<IIndexProvider>();

                    store.RegisterIndexes(indexes);

                    return store;
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

        private static IConfiguration GetStoreConfiguration(IServiceProvider sp, YesSqlOptions yesSqlOptions)
        {
            var storeConfiguration = new YesSql.Configuration
            {
                CommandsPageSize = yesSqlOptions.CommandsPageSize,
                QueryGatingEnabled = yesSqlOptions.QueryGatingEnabled,
                ContentSerializer = new PoolingJsonContentSerializer(sp.GetService<ArrayPool<char>>()),
            };

            if (yesSqlOptions.IdGenerator != null)
            {
                storeConfiguration.IdGenerator = yesSqlOptions.IdGenerator;
            }

            if (yesSqlOptions.TableNameConvention != null)
            {
                storeConfiguration.TableNameConvention = yesSqlOptions.TableNameConvention;
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
