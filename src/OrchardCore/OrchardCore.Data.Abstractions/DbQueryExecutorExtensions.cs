using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace OrchardCore.Data;

public static class DbQueryExecutorExtensions
{
    public static Task ExecuteAsync(this IDbQueryExecutor executor, Func<Task> callback, DbExecutionContext context = null)
        => executor.ExecuteAsync((connection, transaction) => callback(), context ?? DbExecutionContext.Instance);

    public static Task ExecuteAsync(this IDbQueryExecutor executor, Func<DbConnection, Task> callback, DbExecutionContext context = null)
        => executor.ExecuteAsync((connection, transaction) => callback(connection), context ?? DbExecutionContext.Instance);

    public static Task ExecuteAsync(this IDbQueryExecutor executor, Func<DbConnection, DbTransaction, Task> callback)
        => executor.ExecuteAsync(callback, DbExecutionContext.Instance);

    public static async Task<T> QueryAsync<T>(this IDbQueryExecutor queryExecutor, Func<Task<T>> callback, DbExecutionContext context = null)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var result = default(T);

        await queryExecutor.ExecuteAsync(async (connection, transaction) =>
        {
            result = await callback();
        }, context ?? DbExecutionContext.Instance);

        return result;
    }

    public static async Task<T> QueryAsync<T>(this IDbQueryExecutor queryExecutor, Func<DbConnection, Task<T>> callback, DbExecutionContext context = null)
    {
        var result = default(T);

        await queryExecutor.ExecuteAsync(async (connection, transaction) =>
        {
            result = await callback(connection);
        }, context ?? DbExecutionContext.Instance);

        return result;
    }

    public static async Task<T> QueryAsync<T>(this IDbQueryExecutor queryExecutor, Func<DbConnection, DbTransaction, Task<T>> callback, DbExecutionContext context = null)
    {
        var result = default(T);

        await queryExecutor.ExecuteAsync(async (connection, transaction) =>
        {
            result = await callback(connection, transaction);
        }, context ?? DbExecutionContext.Instance);

        return result;
    }
}
