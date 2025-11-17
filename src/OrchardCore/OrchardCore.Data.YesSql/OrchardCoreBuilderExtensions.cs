using System.Buffers;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Data.YesSql;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Json;
using OrchardCore.Modules;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;
using YesSql.Serialization;
using YesSqlSession = YesSql.ISession;

namespace Microsoft.Extensions.DependencyInjection;

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

        builder.ConfigureServices((services, serviceProvider) =>
        {
            var configuration = serviceProvider.GetService<IShellConfiguration>();

            services.Configure<YesSqlOptions>(configuration.GetSection("OrchardCore_YesSql"));
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
                            .UseSqlServer(shellSettings["ConnectionString"], yesSqlOptions.IsolationLevel, shellSettings["Schema"])
                            .UseBlockIdGenerator();
                        break;
                    case DatabaseProviderValue.Sqlite:
                        var shellOptions = sp.GetService<IOptions<ShellOptions>>().Value;
                        var sqliteOptions = sp.GetService<IOptions<SqliteOptions>>().Value;

                        var databaseFolder = SqliteHelper.GetDatabaseFolder(shellOptions, shellSettings.Name);
                        Directory.CreateDirectory(databaseFolder);

                        var connectionString = SqliteHelper.GetConnectionString(sqliteOptions, databaseFolder, shellSettings);

                        storeConfiguration
                            .UseSqLite(connectionString, yesSqlOptions.IsolationLevel)
                            .UseDefaultIdGenerator();
                        break;
                    case DatabaseProviderValue.MySql:
                        storeConfiguration
                            .UseMySql(shellSettings["ConnectionString"], yesSqlOptions.IsolationLevel, shellSettings["Schema"])
                            .UseBlockIdGenerator();
                        break;
                    case DatabaseProviderValue.Postgres:
                        storeConfiguration
                            .UsePostgreSql(shellSettings["ConnectionString"], yesSqlOptions.IsolationLevel, shellSettings["Schema"])
                            .UseBlockIdGenerator();
                        break;
                    default:
                        throw new ArgumentException("Unknown database provider: " + shellSettings["DatabaseProvider"]);
                }

                if (!string.IsNullOrWhiteSpace(shellSettings["TablePrefix"]))
                {
                    var tablePrefix = shellSettings["TablePrefix"].Trim() + databaseTableOptions.TableNameSeparator;

                    storeConfiguration.SetTablePrefix(tablePrefix);
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

                // Get HttpContext to register OnStarting callback and set flags
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor?.HttpContext;
                
                if (httpContext != null)
                {
                    // Set a flag to indicate that ISession was resolved
                    httpContext.Items["OrchardCore:SessionResolved"] = true;

                    // Register OnStarting callback to commit before response
                    try
                    {
                        httpContext.Response.OnStarting(async () =>
                        {
                            // Check if already committed
                            if (httpContext.Items.ContainsKey("OrchardCore:Committed"))
                            {
                                return;
                            }

                            // Check if an exception occurred during the request
                            if (httpContext.Items.ContainsKey("OrchardCore:ExceptionOccurred"))
                            {
                                return;
                            }

                            try
                            {
                                var logger = sp.GetService<ILogger<YesSqlSession>>();
                                
                                // Prefer DocumentStore if it was resolved
                                if (httpContext.Items.ContainsKey("OrchardCore:DocumentStoreResolved"))
                                {
                                    var documentStore = httpContext.RequestServices.GetService<IDocumentStore>();
                                    if (documentStore != null)
                                    {
                                        logger?.LogDebug("Committing IDocumentStore before response");
                                        await documentStore.CommitAsync();
                                        httpContext.Items["OrchardCore:Committed"] = true;
                                    }
                                }
                                else
                                {
                                    var currentSession = httpContext.RequestServices.GetService<YesSqlSession>();
                                    if (currentSession != null)
                                    {
                                        logger?.LogDebug("Committing ISession before response");
                                        await currentSession.SaveChangesAsync();
                                        httpContext.Items["OrchardCore:Committed"] = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var logger = sp.GetService<ILogger<YesSqlSession>>();
                                logger?.LogError(ex, "Failed to commit database changes before response");
                                throw;
                            }
                        });
                    }
                    catch (InvalidOperationException)
                    {
                        // Response may have already started, which is fine
                    }
                }

                // Register automated document store commit and rollback when the ISession is used
                // on the DI scope of the shell. This acts as a fallback for non-HTTP contexts
                // or when the OnStarting callback doesn't commit (e.g., independent ShellScope).
                var shellScope = ShellScope.Current;
                if (sp == shellScope?.ServiceProvider)
                {
                    shellScope
                        .RegisterBeforeDispose(scope =>
                        {
                            // Check if already committed via OnStarting callback
                            if (httpContext?.Items.ContainsKey("OrchardCore:Committed") == true)
                            {
                                // Already committed via OnStarting, skip
                                return Task.CompletedTask;
                            }

                            return scope.ServiceProvider
                                .GetRequiredService<IDocumentStore>()
                                .CommitAsync();
                        })
                        .AddExceptionHandler((scope, e) =>
                        {
                            // Mark that an exception occurred to prevent commit
                            if (httpContext != null)
                            {
                                httpContext.Items["OrchardCore:ExceptionOccurred"] = true;
                            }

                            return scope.ServiceProvider
                                .GetRequiredService<IDocumentStore>()
                                .CancelAsync();
                        });
                }

                return session;
            });

            services.AddScoped<IDocumentStore>(sp =>
            {
                var session = sp.GetService<YesSqlSession>();
                
                if (session == null)
                {
                    return null;
                }
                
                var documentStore = new DocumentStore(session);

                // Get HttpContext to register OnStarting callback and set flags
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor?.HttpContext;
                
                if (httpContext != null)
                {
                    // Set a flag to indicate that IDocumentStore was resolved
                    httpContext.Items["OrchardCore:DocumentStoreResolved"] = true;

                    // Register OnStarting callback to commit before response (if not already registered by ISession)
                    if (!httpContext.Items.ContainsKey("OrchardCore:SessionResolved"))
                    {
                        try
                        {
                            httpContext.Response.OnStarting(async () =>
                            {
                                // Check if already committed
                                if (httpContext.Items.ContainsKey("OrchardCore:Committed"))
                                {
                                    return;
                                }

                                // Check if an exception occurred during the request
                                if (httpContext.Items.ContainsKey("OrchardCore:ExceptionOccurred"))
                                {
                                    return;
                                }

                                try
                                {
                                    var logger = sp.GetService<ILogger<IDocumentStore>>();
                                    logger?.LogDebug("Committing IDocumentStore before response");
                                    await documentStore.CommitAsync();
                                    httpContext.Items["OrchardCore:Committed"] = true;
                                }
                                catch (Exception ex)
                                {
                                    var logger = sp.GetService<ILogger<IDocumentStore>>();
                                    logger?.LogError(ex, "Failed to commit database changes before response");
                                    throw;
                                }
                            });
                        }
                        catch (InvalidOperationException)
                        {
                            // Response may have already started, which is fine
                        }
                    }
                }

                return documentStore;
            });
            services.AddSingleton<IFileDocumentStore, FileDocumentStore>();
            services.AddTransient<IDbConnectionAccessor, DbConnectionAccessor>();
        });

        return builder;
    }

    private static YesSql.Configuration GetStoreConfiguration(IServiceProvider sp, YesSqlOptions yesSqlOptions, DatabaseTableOptions databaseTableOptions)
    {
        var tableNameFactory = sp.GetRequiredService<ITableNameConventionFactory>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var serializerOptions = sp.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>();

        var storeConfiguration = new YesSql.Configuration
        {
            CommandsPageSize = yesSqlOptions.CommandsPageSize,
            QueryGatingEnabled = yesSqlOptions.QueryGatingEnabled,
            EnableThreadSafetyChecks = yesSqlOptions.EnableThreadSafetyChecks,
            TableNameConvention = tableNameFactory.Create(databaseTableOptions),
            IdentityColumnSize = Enum.Parse<IdentityColumnSize>(databaseTableOptions.IdentityColumnSize),
            Logger = loggerFactory.CreateLogger("YesSql"),
            ContentSerializer = new DefaultContentJsonSerializer(serializerOptions.Value.SerializerOptions),
            IsolationLevel = yesSqlOptions.IsolationLevel,
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
