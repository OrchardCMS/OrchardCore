using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Documents;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using YesSql;

namespace OrchardCore.Environment.Shell;

public class ShellDbTablesManager : IShellDbTablesManager
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellDbTablesManager(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        ILogger<ShellDbTablesManager> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    public async Task<ShellDbTablesResult> GetTablesAsync(string tenant)
    {
        var shellDbTablesInfo = new ShellDbTablesInfo
        {
            TenantName = tenant,
        };

        await GetTablesAsync(shellDbTablesInfo);

        return shellDbTablesInfo.GetResult();
    }

    public async Task<ShellDbTablesResult> RemoveTablesAsync(string tenant)
    {
        var shellDbTablesInfo = new ShellDbTablesInfo
        {
            TenantName = tenant,
        };

        await GetTablesAsync(shellDbTablesInfo);
        if (shellDbTablesInfo.Success)
        {
            await RemoveTablesAsync(shellDbTablesInfo);
        }

        return shellDbTablesInfo.GetResult();
    }

    internal async Task GetTablesAsync(ShellDbTablesInfo shellDbTablesInfo)
    {
        if (!_shellHost.TryGetSettings(shellDbTablesInfo.TenantName, out var shellSettings))
        {
            var ex = new InvalidOperationException($"The tenant '{shellDbTablesInfo.TenantName}' doesn't exist.");
            _logger.LogError(ex, "Failed to get the tables of tenant '{TenantName}'.", shellDbTablesInfo.TenantName);
            shellDbTablesInfo.Message = $"Failed to get the tables of tenant '{shellDbTablesInfo.TenantName}'.";
            shellDbTablesInfo.Error = ex;

            return;
        }

        // Create a context composed of all features that have been installed.
        using var shellContext = await GetMaximumContextAsync(shellSettings);
        await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            shellDbTablesInfo.Configure(store.Configuration);

            // Replay tenant migrations but by using a 'ShellDbTablesInfo' that
            // only tracks table definitions without executing any Sql commands.
            var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
            foreach (var migration in migrations)
            {
                migration.SchemaBuilder = shellDbTablesInfo;

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
                        "Failed to replay the migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                        type,
                        version,
                        tenant);

                    shellDbTablesInfo.Message = $"Failed to replay the migration '{type}' from version '{version}' on tenant '{tenant}'.";
                    shellDbTablesInfo.Error = ex.InnerException;

                    break;
                }
            }

            // The 'ShellDbTablesInfo' doesn't execute any Sql command and type definitions
            // are intended to be idempotent, but anyway here nothing needs to be persisted.
            var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            await documentStore.CancelAsync();
        });
    }

    internal async Task RemoveTablesAsync(ShellDbTablesInfo shellDbTablesInfo)
    {
        InvalidOperationException exception = null;
        if (!_shellHost.TryGetSettings(shellDbTablesInfo.TenantName, out var shellSettings))
        {
            exception = new InvalidOperationException($"The tenant '{shellDbTablesInfo.TenantName}' doesn't exist.");
            _logger.LogError(exception, "Failed to remove the tables of tenant '{TenantName}'.", shellDbTablesInfo.TenantName);
        }
        else if (shellSettings.Name == ShellHelper.DefaultShellName)
        {
            exception = new InvalidOperationException("The tenant should not be the 'Default' tenant.");
            _logger.LogError(exception, "Failed to remove the tables of tenant '{TenantName}'.", shellSettings.Name);
        }
        else if (shellSettings.State != TenantState.Disabled)
        {
            exception = new InvalidOperationException($"The tenant '{shellDbTablesInfo.TenantName}' should be 'Disabled'.");
            _logger.LogError(exception, "Failed to remove the tables of tenant '{TenantName}'.", shellSettings.Name);
        }

        if (exception != null)
        {
            shellDbTablesInfo.Message = $"Failed to remove the tables of tenant '{shellSettings.Name}'.";
            shellDbTablesInfo.Error = exception;
            return;
        }

        try
        {
            if (shellSettings["DatabaseProvider"] == "Sqlite")
            {
                ;
            }

            using var shellContext = await GetMinimumContextAsync(shellSettings);
            var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

            using var connection = store.Configuration.ConnectionFactory.CreateConnection();
            if (shellSettings["DatabaseProvider"] == "Sqlite" && connection is SqliteConnection sqliteConnection)
            {
                // Clear the pool to unlock the file and remove it.
                SqliteConnection.ClearPool(sqliteConnection);
                File.Delete(connection.DataSource);
            }
            else
            {
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel);

                // Remove all tables of this tenant.
                shellDbTablesInfo.Configure(transaction);
                //shellDbTablesInfo.RemoveAllTables();

                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove the tables of tenant '{TenantName}'.", shellSettings.Name);
            shellDbTablesInfo.Message = $"Failed to remove the tables of tenant '{shellSettings.Name}'.";
            shellDbTablesInfo.Error = ex;
        }
    }

    private async Task<ShellContext> GetMaximumContextAsync(ShellSettings shellSettings)
    {
        var shellDescriptor = await GetShellDescriptorAsync(shellSettings);
        if (shellDescriptor == null)
        {
            return await GetMinimumContextAsync(shellSettings);
        }

        // Create a shell descriptor composed of all features that have been installed.
        shellDescriptor = new ShellDescriptor { Features = shellDescriptor.Installed };

        return await _shellContextFactory.CreateDescribedContextAsync(shellSettings, shellDescriptor);
    }

    private async Task<ShellDescriptor> GetShellDescriptorAsync(ShellSettings shellSettings)
    {
        ShellDescriptor shellDescriptor = null;

        using var shellContext = await GetMinimumContextAsync(shellSettings);
        await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
        {
            var shellDescriptorManager = scope.ServiceProvider.GetService<IShellDescriptorManager>();
            shellDescriptor = await shellDescriptorManager.GetShellDescriptorAsync();
        });

        return shellDescriptor;
    }

    private Task<ShellContext> GetMinimumContextAsync(ShellSettings shellSettings) =>
        _shellContextFactory.CreateDescribedContextAsync(shellSettings, new ShellDescriptor());

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
