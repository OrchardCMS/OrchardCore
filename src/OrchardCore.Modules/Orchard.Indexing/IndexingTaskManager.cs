using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using YesSql;

namespace Orchard.Indexing.Services
{
    public class IndexingTaskManager : IIndexingTaskManager, IDisposable
    {
        private readonly IClock _clock;
        private readonly IStore _store;
        private readonly string _tablePrefix;
        private readonly List<IndexingTask> _tasksQueue = new List<IndexingTask>();

        public IndexingTaskManager(
            IStore store,
            IClock clock,
            ILogger<IndexingTaskManager> logger)
        {
            _store = store;
            _clock = clock;
            Logger = logger;

            _tablePrefix = store.Configuration.TablePrefix;
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

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);
            var dialect = SqlDialectFactory.For(connection);

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
            }
            catch(Exception e)
            {
                Logger.LogError("An error occured while updating indexing tasks", e);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();
                connection.Close();
            }

            _tasksQueue.Clear();
        }

        public async Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(int afterTaskId, int count)
        {
            await FlushAsync();

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var dialect = SqlDialectFactory.For(connection);
                var sqlBuilder = dialect.CreateBuilder(_tablePrefix);

                sqlBuilder.Select();
                sqlBuilder.Table(nameof(IndexingTask));
                sqlBuilder.Selector("*");
                sqlBuilder.Take(count);
                sqlBuilder.WhereAlso($"{dialect.QuoteForColumnName("Id")} > @Id");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(dialect), new { Id = afterTaskId }, transaction);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while reading indexing tasks: " + e.Message);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();

                connection.Dispose();
            }
        }
    }
}
