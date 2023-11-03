using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace OrchardCore.Data;

public static class DbQueryExecutorExtensions
{
    public static Task ExecuteAsync(this IDbQueryExecutor executor, Func<DbConnection, DbTransaction, Task> callback)
        => executor.ExecuteAsync(callback, new DbExecutionContext());

    public static async Task<T> QueryAsync<T>(this IDbQueryExecutor queryExecutor, Func<DbConnection, Task<T>> callback, DbQueryContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var result = default(T);

        await queryExecutor.QueryAsync(async (connection) =>
        {
            result = await callback(connection);
        }, context);

        return result;
    }

    public static Task<T> QueryAsync<T>(this IDbQueryExecutor executor, Func<DbConnection, Task<T>> callback)
        => executor.QueryAsync<T>(callback, new DbQueryContext());

    public static Task QueryAsync(this IDbQueryExecutor executor, Func<DbConnection, Task> callback)
        => executor.QueryAsync(callback, new DbQueryContext());
}
