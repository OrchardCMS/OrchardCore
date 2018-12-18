using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.Data.Abstractions;

namespace OrchardCore.Indexing.Services
{
    public class IndexingTaskManager : IIndexingTaskManager, IDisposable
    {
        private readonly IClock _clock;
        private readonly IDbConnectionAccessor _connectionAccessor;
        private readonly string _tablePrefix;
        private readonly List<IndexingTask> _tasksQueue = new List<IndexingTask>();

        public IndexingTaskManager(
            IDbConnectionAccessor connectionAccessor,
            IClock clock,
            ILogger<IndexingTaskManager> logger)
        {
            _connectionAccessor = connectionAccessor;
            _clock = clock;
            Logger = logger;

            _tablePrefix = connectionAccessor.TablePrefix;
        }

        public ILogger Logger { get; set; }

        public Task CreateTaskAsync(ContentItem contentItem, IndexingTaskTypes type)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException("contentItem");
            }

            if (contentItem.Id == 0)
            {
                // Ignore that case, when Update is called on a content item which has not be "created" yet

                return Task.CompletedTask;
            }

            var indexingTask = new IndexingTask
            {
                CreatedUtc = _clock.UtcNow,
                ContentItemId = contentItem.ContentItemId,
                Type = type
            };

            lock (_tasksQueue)
            {
                _tasksQueue.Add(indexingTask);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            FlushAsync().Wait();
        }

        private async Task FlushAsync()
        {
            List<IndexingTask> localQueue;

            lock (_tasksQueue)
            {
                localQueue = new List<IndexingTask>(_tasksQueue);
            }

            if (!localQueue.Any())
            {
                return;
            }

            var connection = await _connectionAccessor.GetConnectionAsync();
           
            var dialect = SqlDialectFactory.For(connection);

            var transaction = connection.BeginTransaction();

            try
            {
                var contentItemIds = new HashSet<string>();

                // Remove duplicate tasks, only keep the last one
                for (var i = localQueue.Count; i > 0; i--)
                {
                    var task = localQueue[i - 1];

                    if (contentItemIds.Contains(task.ContentItemId))
                    {
                        localQueue.RemoveAt(i - 1);
                    }
                    else
                    {
                        contentItemIds.Add(task.ContentItemId);
                    }
                }

                // At this point, content items ids should be unique in _taskQueue
                var ids = localQueue.Select(x => x.ContentItemId).ToArray();
                var table = $"{_tablePrefix}{nameof(IndexingTask)}";

                var deleteCmd = $"delete from {dialect.QuoteForTableName(table)} where {dialect.QuoteForColumnName("ContentItemId")} {dialect.InOperator("@Ids")};";
                await connection.ExecuteAsync(deleteCmd, new { Ids = ids }, transaction);

                var insertCmd = $"insert into {dialect.QuoteForTableName(table)} ({dialect.QuoteForColumnName("CreatedUtc")}, {dialect.QuoteForColumnName("ContentItemId")}, {dialect.QuoteForColumnName("Type")}) values (@CreatedUtc, @ContentItemId, @Type);";
                await connection.ExecuteAsync(insertCmd, _tasksQueue, transaction);

                transaction.Commit();              
            }
            catch(Exception e)
            {
                transaction.Rollback();
                Logger.LogError("An error occured while updating indexing tasks", e);
                throw;
            }
            finally
            {
                connection.Close();
            }

            _tasksQueue.Clear();
        }

        public async Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(int afterTaskId, int count)
        {
            await FlushAsync();

            try
            {
                using (var connection = await _connectionAccessor.GetConnectionAsync())
                {
                    var dialect = SqlDialectFactory.For(connection);
                    var sqlBuilder = dialect.CreateBuilder(_tablePrefix);

                    sqlBuilder.Select();
                    sqlBuilder.Table(nameof(IndexingTask));
                    sqlBuilder.Selector("*");

                    if (count > 0)
                    {
                        sqlBuilder.Take(count.ToString());
                    }

                    sqlBuilder.WhereAlso($"{dialect.QuoteForColumnName("Id")} > @Id");

                    return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while reading indexing tasks");
                throw;
            }
        }
    }
}
