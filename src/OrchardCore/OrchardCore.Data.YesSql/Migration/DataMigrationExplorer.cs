using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using YesSql;

namespace OrchardCore.Data.Migration;

/// <summary>
/// Allows to explores the tables from tenant data migrations.
/// </summary>
public class DataMigrationExplorer : IDataMigrationExplorer
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;

    public DataMigrationExplorer(IShellHost shellHost, IShellContextFactory shellContextFactory)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
    }

    /// <summary>
    /// Retrieves the tables from the data migrations of this tenant.
    /// </summary>
    public Task<SchemaExplorerResult> GetTablesAsync(string tenant) => ExploreTablesAsync(tenant);

    /// <summary>
    /// Removes the tables retrieved from the data migrations of this tenant.
    /// </summary>
    public Task<SchemaExplorerResult> RemoveTablesAsync(string tenant) => ExploreTablesAsync(tenant, remove: true);

    internal async Task<SchemaExplorerResult> ExploreTablesAsync(string tenant, bool remove = false)
    {
        var schemaExplorer = new SchemaExplorer();

        if (_shellHost.TryGetSettings(tenant, out var shellSettings))
        {
            var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
            var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

            schemaExplorer.Configure(store.Configuration);

            // Create a full context composed of all features that was installed on this tenant.
            var fullDescriptor = new ShellDescriptor { Features = shellContext.Blueprint.Descriptor.Installed };
            using var fullContext = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, fullDescriptor);

            await fullContext.CreateScope().UsingServiceScopeAsync(async scope =>
            {
                // Replay all tenant migrations but by using a 'SchemaExplorer'.
                var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
                foreach (var migration in migrations)
                {
                    migration.SchemaBuilder = schemaExplorer;

                    var version = 0;
                    try
                    {
                        version = await ExecuteCreateMethodAsync(migration);
                        version = await ExecuteUpdateMethodsAsync(migration, version);
                    }
                    catch (Exception ex)
                    {
                        var migrationType = migration.GetType().FullName;

                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataMigrationExplorer>>();
                        logger.LogError(
                            ex,
                            "Failed to explore migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                            migrationType,
                            version,
                            tenant);

                        schemaExplorer.Message =
                            $"Failed to explore migration '{migrationType}' from version '{version}' on tenant '{tenant}'.";

                        schemaExplorer.Error = ex.InnerException;

                        break;
                    }
                }
            });

            if (schemaExplorer.Success && remove)
            {
                try
                {
                    using var connection = store.Configuration.ConnectionFactory.CreateConnection();
                    await connection.OpenAsync();
                    using var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel);

                    // Remove all tables of this tenant.
                    schemaExplorer.Configure(transaction);
                    schemaExplorer.RemoveAllTables();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    var logger = shellContext.ServiceProvider.GetRequiredService<ILogger<DataMigrationExplorer>>();
                    logger.LogError(ex, "Failed to rollback migrations of tenant '{TenantName}'.", tenant);

                    schemaExplorer.Message = $"Failed to rollback migrations of tenant '{tenant}'.";
                    schemaExplorer.Error = ex;
                }
            }
        }

        return new SchemaExplorerResult
        {
            TenantName = tenant,
            TablePrefix = schemaExplorer.TablePrefix,
            TableNames = schemaExplorer.GetTableNames(),
            Message = schemaExplorer.Message,
            Error = schemaExplorer.Error,
        };
    }

    private static async Task<int> ExecuteCreateMethodAsync(IDataMigration migration)
    {
        var methodInfo = GetCreateMethod(migration);
        if (methodInfo != null)
        {
            return (int)methodInfo.Invoke(migration, Array.Empty<object>());
        }
        else
        {
            methodInfo = GetCreateAsyncMethod(migration);
            if (methodInfo != null)
            {
                return await(Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
            }
        }

        return 0;
    }

    private static async Task<int> ExecuteUpdateMethodsAsync(IDataMigration migration, int version)
    {
        var updateMethods = GetUpdateMethods(migration);
        while (updateMethods.TryGetValue(version, out var methodInfo))
        {
            var isAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
            if (isAwaitable)
            {
                version = await (Task<int>)methodInfo.Invoke(migration, new object[0]);
            }
            else
            {
                version = (int)methodInfo.Invoke(migration, new object[0]);
            }
        }

        return version;
    }

    private static MethodInfo GetCreateMethod(IDataMigration dataMigration)
    {
        var methodInfo = dataMigration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo != null && methodInfo.ReturnType == typeof(int))
        {
            return methodInfo;
        }

        return null;
    }

    private static MethodInfo GetCreateAsyncMethod(IDataMigration dataMigration)
    {
        var methodInfo = dataMigration.GetType().GetMethod("CreateAsync", BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo != null && methodInfo.ReturnType == typeof(Task<int>))
        {
            return methodInfo;
        }

        return null;
    }

    private static Dictionary<int, MethodInfo> GetUpdateMethods(IDataMigration dataMigration)
    {
        return dataMigration
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(GetUpdateMethod)
            .Where(update => update.MethodInfo != null)
            .ToDictionary(update => update.Version, update => update.MethodInfo);
    }

    private static (int Version, MethodInfo MethodInfo) GetUpdateMethod(MethodInfo methodInfo)
    {
        if (methodInfo.Name.StartsWith("UpdateFrom", StringComparison.Ordinal) &&
            (methodInfo.ReturnType == typeof(int) || methodInfo.ReturnType == typeof(Task<int>)))
        {
            var version = methodInfo.Name.EndsWith("Async", StringComparison.Ordinal)
                ? methodInfo.Name["UpdateFrom".Length..^"Async".Length]
                : methodInfo.Name["UpdateFrom".Length..];

            if (Int32.TryParse(version, out var versionValue))
            {
                return (versionValue, methodInfo);
            }
        }

        return (0, null);
    }
}
