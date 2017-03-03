using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using YesSql.Core.Services;

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
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);
            
            try
            {
                var table = $"{_tablePrefix }{ nameof(IndexingTask)}";

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

                var deleteCmd = $"delete from [{table}] where [ContentItemId] in @Ids;";
                await connection.ExecuteAsync(deleteCmd, new { Ids = ids }, transaction);

                var insertCmd = $"insert into [{table}] ([CreatedUtc], [ContentItemId], [Type]) values (@CreatedUtc, @ContentItemId, @Type);";
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

                if (_store.Configuration.ConnectionFactory.Disposable)
                {
                    connection.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }

            _tasksQueue.Clear();
        }

        public async Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(int afterTaskId, int count)
        {
            await FlushAsync();

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(IndexingTask)}";

                return await connection.QueryAsync<IndexingTask>($"select top {count} * from [{table}] where Id > @Id", new { Id = afterTaskId }, transaction);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while reading indexing tasks", e);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();

                if (_store.Configuration.ConnectionFactory.Disposable)
                {
                    connection.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }
        }
    }
}
