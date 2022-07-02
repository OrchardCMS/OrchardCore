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

public class ShellDbTablesManager : IShellDbTablesManager
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public ShellDbTablesManager(IShellHost shellHost, IShellContextFactory shellContextFactory, ILogger<ShellDbTablesManager> logger)
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
            _logger.LogError(ex, "The tenant '{TenantName}' doesn't exist.", shellDbTablesInfo.TenantName);
            shellDbTablesInfo.Error = ex;

            return;
        }

        var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
        var store = shellContext.ServiceProvider.GetRequiredService<IStore>();

        shellDbTablesInfo.Configure(store.Configuration);

        // Create a full context composed of all features that have been installed on this tenant.
        var descriptor = new ShellDescriptor { Features = shellContext.Blueprint.Descriptor.Installed };

        using var context = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, descriptor);
        await context.CreateScope().UsingServiceScopeAsync(async scope =>
        {
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
                        "Failed to replay migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                        type,
                        version,
                        tenant);

                    shellDbTablesInfo.Message = $"Failed to replay migration '{type}' from version '{version}' on tenant '{tenant}'.";
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
        if (!_shellHost.TryGetSettings(shellDbTablesInfo.TenantName, out var shellSettings))
        {
            var ex = new InvalidOperationException($"The tenant '{shellDbTablesInfo.TenantName}' doesn't exist.");
            _logger.LogError(ex, "The tenant '{TenantName}' doesn't exist.", shellDbTablesInfo.TenantName);
            shellDbTablesInfo.Error = ex;

            return;
        }
        else if (shellSettings.Name == ShellHelper.DefaultShellName)
        {
            var ex = new InvalidOperationException("Can't remove the tables of the 'Default' tenant.");
            _logger.LogError(ex, "Can't remove the tables of the 'Default' tenant");
            shellDbTablesInfo.Error = ex;

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
            shellDbTablesInfo.Configure(transaction);
            shellDbTablesInfo.RemoveAllTables();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove tables on tenant '{TenantName}'.", shellSettings.Name);
            shellDbTablesInfo.Message = $"Failed to remove tables on tenant '{shellSettings.Name}'.";
            shellDbTablesInfo.Error = ex;
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
