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

public class DataMigrationAnalyzer : IDataMigrationAnalyzer
{
    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly ILogger _logger;

    public DataMigrationAnalyzer(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        ILogger<DataMigrationAnalyzer> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _logger = logger;
    }

    public async Task<DataMigrationAnalyzerResult> AnalyzeAsync(string tenantName)
    {
        var result = new DataMigrationAnalyzerResult { TenantName = tenantName };
        if (_shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            var tenantShell = await _shellHost.GetOrCreateShellContextAsync(shellSettings);
            var descriptor = new ShellDescriptor { Features = tenantShell.Blueprint.Descriptor.Installed };

            using var shellContext = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, descriptor);
            await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
            {
                var store = scope.ServiceProvider.GetRequiredService<IStore>();
                var schemaAnalyzer = new SchemaAnalyzer(store.Configuration);

                var migrations = scope.ServiceProvider.GetServices<IDataMigration>();
                foreach (var migration in migrations)
                {
                    migration.SchemaBuilder = schemaAnalyzer;

                    var current = 0;
                    try
                    {
                        var methodInfo = GetCreateMethod(migration);
                        if (methodInfo != null)
                        {
                            current = (int)methodInfo.Invoke(migration, Array.Empty<object>());
                        }
                        else
                        {
                            methodInfo = GetCreateAsyncMethod(migration);
                            if (methodInfo != null)
                            {
                                await (Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
                            }
                        }

                        var lookupTable = CreateUpgradeLookupTable(migration);
                        while (lookupTable.TryGetValue(current, out methodInfo))
                        {
                            var isAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
                            if (isAwaitable)
                            {
                                current = await (Task<int>)methodInfo.Invoke(migration, new object[0]);
                            }
                            else
                            {
                                current = (int)methodInfo.Invoke(migration, new object[0]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to analyze migration '{MigrationType}' from version '{Version}' on tenant '{TenantName}'.",
                            migration.GetType().FullName,
                            current,
                            tenantName);

                        result.Message =
                            $"Failed to analyze migration '{migration.GetType().FullName}' from version '{current}' on tenant '{tenantName}'.";

                        result.Error = ex.InnerException;

                        break;
                    }
                }

                result.TablePrefix = schemaAnalyzer.TablePrefix;
                result.Tables = schemaAnalyzer.Tables;
                result.IndexTables = schemaAnalyzer.IndexTables;
                result.BridgeTables = schemaAnalyzer.BridgeTables;
                result.DocumentTables = schemaAnalyzer.DocumentTables;
                result.Collections = schemaAnalyzer.Collections;
            });
        }

        return result;
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

    private static Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration)
    {
        return dataMigration
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(GetUpdateMethod)
            .Where(tuple => tuple != null)
            .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
    }

    private static Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo methodInfo)
    {
        const string updateFromPrefix = "UpdateFrom";
        const string asyncSuffix = "Async";

        if (methodInfo.Name.StartsWith(updateFromPrefix, StringComparison.Ordinal) &&
            (methodInfo.ReturnType == typeof(int) ||
                methodInfo.ReturnType == typeof(Task<int>)))
        {
            var version = methodInfo.Name.EndsWith(asyncSuffix, StringComparison.Ordinal)
                ? methodInfo.Name.Substring(updateFromPrefix.Length, methodInfo.Name.Length - updateFromPrefix.Length - asyncSuffix.Length)
                : methodInfo.Name.Substring(updateFromPrefix.Length);

            if (Int32.TryParse(version, out var versionValue))
            {
                return new Tuple<int, MethodInfo>(versionValue, methodInfo);
            }
        }

        return null;
    }
}
