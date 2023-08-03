using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OrchardCore.Data.Migration;

public static class DataMigrationExtensions
{
    /// <summary>
    /// Executes the create method of the provided data migration. 
    /// </summary>
    public static async Task<int> ExecuteCreateMethodAsync(this IDataMigration migration)
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

    /// <summary>
    /// Executes the update methods of the provided data migration. 
    /// </summary>
    public static async Task<int> ExecuteUpdateMethodsAsync(this IDataMigration migration, int version)
    {
        var updateMethods = GetUpdateMethods(migration);
        while (updateMethods.TryGetValue(version, out var methodInfo))
        {
            var isAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
            if (isAwaitable)
            {
                version = await (Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
            }
            else
            {
                version = (int)methodInfo.Invoke(migration, Array.Empty<object>());
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
