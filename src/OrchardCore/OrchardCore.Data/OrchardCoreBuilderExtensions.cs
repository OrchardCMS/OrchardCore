using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static IApplicationBuilder UseDataAccess(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CommitSessionMiddleware>();
        }

        /// <summary>
        /// Adds tenant level data access services.
        /// </summary>
        public static OrchardCoreBuilder AddDataAccess(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IDataMigrationManager, DataMigrationManager>();
                services.AddScoped<IModularTenantEvents, AutomaticDataMigrations>();

                // Adding supported databases
                services.TryAddDataProvider(name: "Sql Server", value: "SqlConnection", hasConnectionString: true, hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Sqlite", value: "Sqlite", hasConnectionString: false, hasTablePrefix: false, isDefault: true);
                services.TryAddDataProvider(name: "MySql", value: "MySql", hasConnectionString: true, hasTablePrefix: true, isDefault: false);
                services.TryAddDataProvider(name: "Postgres", value: "Postgres", hasConnectionString: true, hasTablePrefix: true, isDefault: false);

                // Configuring data access

                services.AddSingleton<IStore>(sp =>
                {
                    var shellSettings = sp.GetService<ShellSettings>();

                    if (shellSettings.DatabaseProvider == null)
                    {
                        return null;
                    }

                    var storeConfiguration = new YesSql.Configuration();

                    // Disabling query gating as it's failing to improve performance right now
                    //storeConfiguration.DisableQueryGating();

                    switch (shellSettings.DatabaseProvider)
                    {
                        case "SqlConnection":
                            storeConfiguration.UseSqlServer(shellSettings.ConnectionString, IsolationLevel.ReadUncommitted);
                            break;
                        case "Sqlite":
                            var shellOptions = sp.GetService<IOptions<ShellOptions>>();
                            var option = shellOptions.Value;
                            var databaseFolder = Path.Combine(option.ShellsApplicationDataPath, option.ShellsContainerName, shellSettings.Name);
                            var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                            Directory.CreateDirectory(databaseFolder);
                            storeConfiguration.UseSqLite($"Data Source={databaseFile};Cache=Shared", IsolationLevel.ReadUncommitted);
                            break;
                        case "MySql":
                            storeConfiguration.UseMySql(shellSettings.ConnectionString, IsolationLevel.ReadUncommitted);
                            break;
                        case "Postgres":
                            storeConfiguration.UsePostgreSql(shellSettings.ConnectionString, IsolationLevel.ReadUncommitted);
                            break;
                        default:
                            throw new ArgumentException("Unknown database provider: " + shellSettings.DatabaseProvider);
                    }

                    if (!string.IsNullOrWhiteSpace(shellSettings.TablePrefix))
                    {
                        storeConfiguration.TablePrefix = shellSettings.TablePrefix + "_";
                    }

                    var store = new Store(storeConfiguration);
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

                    var httpContext = sp.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

                    if (httpContext != null)
                    {
                        httpContext.Items[typeof(YesSql.ISession)] = session;
                    }

                    return session;
                });
            });

            return builder;
        }
    }

    public class CommitSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CommitSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            // Don't resolve to prevent instantiating one in case of static sites
            var session = httpContext.Items[typeof(YesSql.ISession)] as YesSql.ISession;

            if (session != null)
            {
                try
                {
                    await session.CommitAsync();
                }
                catch (ObjectDisposedException)
                {
                    // The session might already be disposed by the ServiceScope
                }
            }
        }
    }
}