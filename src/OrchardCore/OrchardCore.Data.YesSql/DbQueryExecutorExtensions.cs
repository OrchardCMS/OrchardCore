using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using YesSql;

namespace OrchardCore.Data;

public static class DbQueryExecutorExtensions
{
    public static Task ExecuteAsync(this IDbQueryExecutor executor, string sql, object parameters = null, DbExecutionContext context = null)
        => executor.ExecuteAsync(async (connection, transaction) =>
        {
            await connection.ExecuteAsync(sql, parameters, transaction);
        }, context ?? DbExecutionContext.Instance);

    public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbQueryExecutor executor, string sql, object parameters = null, DbExecutionContext context = null)
    {
        IEnumerable<T> result = null;
        await executor.ExecuteAsync(async (connection, transaction) =>
        {
            result = await connection.QueryAsync<T>(sql, parameters, transaction);
        }, context ?? DbExecutionContext.Instance);

        return result ?? Enumerable.Empty<T>();
    }

    public static Task<IEnumerable<T>> QueryAsync<T>(this IDbQueryExecutor executor, ISqlBuilder builder, DbExecutionContext context = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return executor.QueryAsync<T>(builder.ToSqlString(), builder.Parameters, context ?? DbExecutionContext.Instance);
    }
}
