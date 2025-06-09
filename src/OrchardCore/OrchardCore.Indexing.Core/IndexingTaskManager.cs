using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Indexing.Services;

/// <summary>
/// This is a scoped service that enlists tasks to be stored in the database.
/// It enlists a final task using the current <see cref="ShellScope"/> such
/// that multiple calls to <see cref="CreateTaskAsync"/> can be done without incurring
/// a SQL query on every single one.
/// </summary>
public sealed class IndexingTaskManager : IIndexingTaskManager
{
    private const int _batchSize = 100;

    private readonly IClock _clock;
    private readonly IStore _store;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly ILogger _logger;

    private readonly List<IndexingTask> _tasksQueue = [];

    public IndexingTaskManager(
        IClock clock,
        IStore store,
        IDbConnectionAccessor dbConnectionAccessor,
        ILogger<IndexingTaskManager> logger)
    {
        _clock = clock;
        _store = store;
        _dbConnectionAccessor = dbConnectionAccessor;
        _logger = logger;
    }

    public Task CreateTaskAsync(CreateIndexingTaskContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var indexingTask = new IndexingTask
        {
            CreatedUtc = _clock.UtcNow,
            RecordId = context.RecordId,
            Category = context.Category,
            Type = context.Type,
        };

        if (_tasksQueue.Count == 0)
        {
            var tasksQueue = _tasksQueue;

            // Using a local var prevents the lambda from holding a ref on this scoped service.
            ShellScope.AddDeferredTask(scope => FlushAsync(scope, tasksQueue));
        }

        _tasksQueue.Add(indexingTask);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(long afterTaskId, int count, string category)
    {
        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();

        try
        {
            var dialect = _store.Configuration.SqlDialect;
            var sqlBuilder = dialect.CreateBuilder(_store.Configuration.TablePrefix);

            sqlBuilder.Select();
            sqlBuilder.Table(nameof(IndexingTask), alias: null, _store.Configuration.Schema);
            sqlBuilder.Selector("*");

            if (count > 0)
            {
                sqlBuilder.Take(count.ToString());
            }

            sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Id")} > @Id");
            sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Category")} = @Category");

            // It is important to sort the tasks by Id to ensure that the tasks are processed in the order
            // they are created as the sql server does not guarantee ordering.
            sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

            return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(),
                new
                {
                    Id = afterTaskId,
                    Category = category,
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while reading indexing tasks");
            throw;
        }
    }

    private async Task FlushAsync(ShellScope scope, List<IndexingTask> tasks)
    {
        var localQueue = new List<IndexingTask>(tasks);

        var serviceProvider = scope.ServiceProvider;

        var store = serviceProvider.GetService<IStore>();
        var dbConnectionAccessor = serviceProvider.GetService<IDbConnectionAccessor>();
        var logger = serviceProvider.GetService<ILogger<IndexingTaskManager>>();

        var taskGroups = new Dictionary<string, HashSet<string>>();

        // Remove duplicate tasks, only keep the last one.
        for (var i = localQueue.Count; i > 0; i--)
        {
            var task = localQueue[i - 1];

            if (!taskGroups.TryGetValue(task.Category, out var recordIds))
            {
                recordIds = [];

                taskGroups[task.Category] = recordIds;
            }

            if (recordIds.Contains(task.RecordId))
            {
                localQueue.RemoveAt(i - 1);
            }
            else
            {
                recordIds.Add(task.RecordId);
            }
        }

        var table = $"{store.Configuration.TablePrefix}{nameof(IndexingTask)}";

        await using var connection = dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);
        var dialect = store.Configuration.SqlDialect;

        try
        {
            foreach (var taskGroup in taskGroups)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Updating indexing tasks record id: '{RecordId}' in '{Category}' category", string.Join(", ", taskGroup.Value), taskGroup.Key);
                }

                var deleteCmd = $"delete from {dialect.QuoteForTableName(table, store.Configuration.Schema)} where {dialect.QuoteForColumnName("Category")} = @Category and {dialect.QuoteForColumnName("RecordId")} {dialect.InOperator("@Ids")};";

                var lapsCounter = 0;

                while (true)
                {
                    // The ids has to be an implementation of IList<T> to be compatible with Dapper's In operator.
                    var pageOfIds = taskGroup.Value.Skip(lapsCounter++ * _batchSize).Take(_batchSize).ToArray();

                    if (pageOfIds.Length == 0)
                    {
                        break;
                    }

                    await transaction.Connection.ExecuteAsync(deleteCmd,
                        new
                        {
                            Category = taskGroup.Key,
                            Ids = pageOfIds,
                        }, transaction);
                }

                var insertCmd = $"insert into {dialect.QuoteForTableName(table, store.Configuration.Schema)} ({dialect.QuoteForColumnName("CreatedUtc")}, {dialect.QuoteForColumnName("RecordId")}, {dialect.QuoteForColumnName("Category")}, {dialect.QuoteForColumnName("Type")}) values (@CreatedUtc, @RecordId, @Category, @Type);";
                await transaction.Connection.ExecuteAsync(insertCmd, localQueue, transaction);
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "An error occurred while updating indexing tasks");

            throw;
        }
    }
}
