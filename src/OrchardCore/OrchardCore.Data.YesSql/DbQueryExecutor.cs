using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Data;

public class DbQueryExecutor : IDbQueryExecutor
{
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly ILogger _logger;

    public DbQueryExecutor(
        IDbConnectionAccessor dbConnectionAccessor,
        ILogger<DbQueryExecutor> logger)
    {
        _dbConnectionAccessor = dbConnectionAccessor;
        _logger = logger;
    }

    public async Task ExecuteAsync(Func<DbConnection, DbTransaction, Task> callback, DbExecutionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await QueryAsync(async (connection) =>
        {
            using var transaction = await connection.BeginTransactionAsync(context.TransactionIsolationLevel, context.CancellationToken);
            try
            {
                await callback(connection, transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to commit a SQL transaction. Transaction was rolled back. {0}", ex.Message);

                await transaction.RollbackAsync();

                if (context.ThrowException)
                {
                    throw;
                }
            }
        }, context);
    }

    public async Task QueryAsync(Func<DbConnection, Task> callback, DbQueryContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await using var connection = _dbConnectionAccessor.CreateConnection();
        try
        {
            await connection.OpenAsync();
            await callback(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to open a connection to the database. {0}", ex.Message);

            if (context.ThrowException)
            {
                throw;
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
