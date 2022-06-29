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

public class DataMigrationRemover : IDataMigrationRemover
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;

    public DataMigrationRemover(IShellHost shellHost, IShellContextFactory shellContextFactory)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
    }

    public Task<RemoveSchemaResult> GetTablesOnlyAsync(string tenantName) => RemoveTablesAsync(tenantName, remove: false);

    public Task<RemoveSchemaResult> RemoveTablesAsync(string tenantName) => RemoveTablesAsync(tenantName, remove: true);

    internal async Task<RemoveSchemaResult> RemoveTablesAsync(string tenantName, bool remove = false)
    {
        var removeSchemaResult = new RemoveSchemaResult { TenantName = tenantName };

        if (_shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            var tenantShell = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
            var descriptor = new ShellDescriptor { Features = tenantShell.Blueprint.Descriptor.Installed };

            using var shellContext = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, descriptor);
            await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var removeSchemaBuilder = new RemoveSchemaBuilder(session.Store.Configuration, transaction: null);
                removeSchemaResult.TablePrefix = removeSchemaBuilder.TablePrefix;

                var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
                foreach (var migration in migrations)
                {
                    migration.SchemaBuilder = removeSchemaBuilder;

                    var version = 0;
                    try
                    {
                        var methodInfo = GetCreateMethod(migration);
                        if (methodInfo != null)
                        {
                            version = (int)methodInfo.Invoke(migration, Array.Empty<object>());
                        }
                        else
                        {
                            methodInfo = GetCreateAsyncMethod(migration);
                            if (methodInfo != null)
                            {
                                await (Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
                            }
                        }

                        var updateMethods = GetUpdateMethods(migration);
                        while (updateMethods.TryGetValue(version, out methodInfo))
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
                    }
                    catch (Exception ex)
                    {
                        var migrationType = migration.GetType().FullName;

                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataMigrationRemover>>();
                        logger.LogError(
                            ex,
                            "Failed to analyze migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                            migrationType,
                            version,
                            tenantName);

                        removeSchemaResult.Message =
                            $"Failed to analyze migration '{migrationType}' from version '{version}' on tenant '{tenantName}'.";

                        removeSchemaResult.Error = ex.InnerException;

                        break;
                    }
                }

                removeSchemaResult.TableNames = removeSchemaBuilder.GetTableNames();

                if (!remove)
                {
                    return;
                }

                try
                {
                    var transaction = await session.BeginTransactionAsync();
                    removeSchemaBuilder.Transaction = transaction;
                    removeSchemaBuilder.Connection = transaction.Connection;

                    removeSchemaBuilder.RemoveMapIndexTables();
                    removeSchemaBuilder.RemoveReduceIndexTables();
                    removeSchemaBuilder.RemoveTables();

                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataMigrationRemover>>();
                    logger.LogError(ex, "Failed to remove tables of tenant '{TenantName}'.", tenantName);

                    removeSchemaResult.Message = $"Failed to remove tables of tenant '{tenantName}'.";
                    removeSchemaResult.Error = ex;
                }

            });
        }

        return removeSchemaResult;
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
