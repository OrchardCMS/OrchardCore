using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using YesSql;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Allows to retrieve and remove all the migrated tables of a given tenant.
/// </summary>
public class ShellTablesService : IShellTablesService
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellTablesService(IShellHost shellHost, IShellContextFactory shellContextFactory, ILogger<ShellTablesService> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all the migrated tables of the provided tenant.
    /// </summary>
    public async Task<ShellTablesResult> GetTablesAsync(string tenant)
    {
        var shellTablesRecorder = new ShellTablesRecorder
        {
            TenantName = tenant,
        };

        await GetTablesAsync(shellTablesRecorder);

        return new ShellTablesResult
        {
            TenantName = shellTablesRecorder.TenantName,
            TablePrefix = shellTablesRecorder.TablePrefix,
            TableNames = shellTablesRecorder.GetTableNames(),
            Message = shellTablesRecorder.Message,
            Error = shellTablesRecorder.Error,
        };
    }

    /// <summary>
    /// Removes all the migrated tables of the provided tenant.
    /// </summary>
    public async Task<ShellTablesResult> RemoveTablesAsync(string tenant)
    {
        var shellTablesRecorder = new ShellTablesRecorder
        {
            TenantName = tenant,
        };

        await GetTablesAsync(shellTablesRecorder);
        if (shellTablesRecorder.Success)
        {
            await RemoveTablesAsync(shellTablesRecorder);
        }

        return new ShellTablesResult
        {
            TenantName = shellTablesRecorder.TenantName,
            TablePrefix = shellTablesRecorder.TablePrefix,
            TableNames = shellTablesRecorder.GetTableNames(),
            Message = shellTablesRecorder.Message,
            Error = shellTablesRecorder.Error,
        };
    }

    internal async Task GetTablesAsync(ShellTablesRecorder shellTablesRecorder)
    {
        if (!_shellHost.TryGetSettings(shellTablesRecorder.TenantName, out var shellSettings))
        {
            var ex = new InvalidOperationException("The provided tenant doesn't exist.");
            _logger.LogError(ex, "Failed to retrieve tables of tenant '{TenantName}'.", shellTablesRecorder.TenantName);
            shellTablesRecorder.Message = $"Failed to retrieve tables of tenant '{shellTablesRecorder.TenantName}'.";
            shellTablesRecorder.Error = ex;

            return;
        }

        var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
        var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

        shellTablesRecorder.Configure(store.Configuration);

        // Create a full context composed of all features that have been installed on this tenant.
        var descriptor = new ShellDescriptor { Features = shellContext.Blueprint.Descriptor.Installed };

        using var context = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, descriptor);
        await context.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            // Replay tenant migrations but by using a 'ShellTablesRecorder' that
            // only records table definitions without executing any Sql commands.
            var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
            foreach (var migration in migrations)
            {
                migration.SchemaBuilder = shellTablesRecorder;

                var version = 0;
                try
                {
                    version = await ExecuteCreateMethodAsync(migration);
                    version = await ExecuteUpdateMethodsAsync(migration, version);
                }
                catch (Exception ex)
                {
                    var type = migration.GetType().FullName;
                    var tenant = shellSettings.Name;

                    _logger.LogError(
                        ex,
                        "Failed to replay migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                        type,
                        version,
                        tenant);

                    shellTablesRecorder.Message = $"Failed to replay migration '{type}' from version '{version}' on tenant '{tenant}'.";
                    shellTablesRecorder.Error = ex.InnerException;

                    break;
                }
            }

            // The 'ShellTablesRecorder' doesn't execute any Sql commands and content
            // definitions are idempotent, but anyway here nothing needs to be stored.
            var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            await documentStore.CancelAsync();
        });
    }

    internal async Task RemoveTablesAsync(ShellTablesRecorder shellTablesRecorder)
    {
        if (!_shellHost.TryGetSettings(shellTablesRecorder.TenantName, out var shellSettings))
        {
            var ex = new InvalidOperationException("The provided tenant doesn't exist.");
            _logger.LogError(ex, "Failed to remove tables of tenant '{TenantName}'.", shellTablesRecorder.TenantName);
            shellTablesRecorder.Message = $"Failed to remove tables of tenant '{shellTablesRecorder.TenantName}'.";
            shellTablesRecorder.Error = ex;

            return;
        }

        var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
        var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

        try
        {
            using var connection = store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel);

            // Remove all tables of this tenant.
            shellTablesRecorder.Configure(transaction);
            shellTablesRecorder.RemoveAllTables();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to to remove tables of tenant '{TenantName}'.", shellSettings.Name);
            shellTablesRecorder.Message = $"Failed to remove tables of tenant '{shellSettings.Name}'.";
            shellTablesRecorder.Error = ex;
        }
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
                return await (Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
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
