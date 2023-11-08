using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentPreview;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Indexing.Services
{
    /// <summary>
    /// This is a scoped service that enlists tasks to be stored in the database.
    /// It enlists a final task using the current <see cref="ShellScope"/> such
    /// that multiple calls to <see cref="CreateTaskAsync"/> can be done without incurring
    /// a SQL query on every single one.
    /// </summary>
    public class IndexingTaskManager : IIndexingTaskManager
    {
        private readonly IClock _clock;
        private readonly IStore _store;
        private readonly IDbQueryExecutor _queryExecutor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        private readonly List<IndexingTask> _tasksQueue = new();

        public IndexingTaskManager(
            IClock clock,
            IStore store,
            IDbQueryExecutor queryExecutor,
            IHttpContextAccessor httpContextAccessor,
            ILogger<IndexingTaskManager> logger)
        {
            _clock = clock;
            _store = store;
            _queryExecutor = queryExecutor;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task CreateTaskAsync(ContentItem contentItem, IndexingTaskTypes type)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            // Do not index a preview content item.
            if (_httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing == true)
            {
                return Task.CompletedTask;
            }

            if (contentItem.Id == 0)
            {
                // Ignore that case, when Update is called on a content item which has not be "created" yet.
                return Task.CompletedTask;
            }

            var indexingTask = new IndexingTask
            {
                CreatedUtc = _clock.UtcNow,
                ContentItemId = contentItem.ContentItemId,
                Type = type
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

        private async Task FlushAsync(ShellScope scope, IEnumerable<IndexingTask> tasks)
        {
            var localQueue = new List<IndexingTask>(tasks);

            var serviceProvider = scope.ServiceProvider;

            var session = serviceProvider.GetService<YesSql.ISession>();
            var dbConnectionAccessor = serviceProvider.GetService<IDbConnectionAccessor>();
            var shellSettings = serviceProvider.GetService<ShellSettings>();
            var logger = serviceProvider.GetService<ILogger<IndexingTaskManager>>();

            var contentItemIds = new HashSet<string>();

            // Remove duplicate tasks, only keep the last one.
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

            // At this point, content items ids should be unique in localQueue.
            var ids = localQueue.Select(x => x.ContentItemId).ToList();
            var table = $"{session.Store.Configuration.TablePrefix}{nameof(IndexingTask)}";

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Updating indexing tasks: {ContentItemIds}", string.Join(", ", tasks.Select(x => x.ContentItemId)));
            }

            // Page delete statements to prevent the limits from IN sql statements.
            await _queryExecutor.ExecuteAsync(async (connection, transaction) =>
            {
                var pageSize = 100;
                var dialect = session.Store.Configuration.SqlDialect;

                var deleteCmd = $"delete from {dialect.QuoteForTableName(table, _store.Configuration.Schema)} where {dialect.QuoteForColumnName("ContentItemId")} {dialect.InOperator("@Ids")};";

                do
                {
                    var pageOfIds = ids.Take(pageSize).ToList();

                    if (pageOfIds.Count > 0)
                    {
                        await connection.ExecuteAsync(deleteCmd, new { Ids = pageOfIds }, transaction);
                        ids = ids.Skip(pageSize).ToList();
                    }
                } while (ids.Count > 0);

                var insertCmd = $"insert into {dialect.QuoteForTableName(table, _store.Configuration.Schema)} ({dialect.QuoteForColumnName("CreatedUtc")}, {dialect.QuoteForColumnName("ContentItemId")}, {dialect.QuoteForColumnName("Type")}) values (@CreatedUtc, @ContentItemId, @Type);";
                await connection.ExecuteAsync(insertCmd, localQueue, transaction);
            });
        }

        public async Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(long afterTaskId, int count)
            => await _queryExecutor.QueryAsync(async (connection) =>
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
                sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
            });
    }
}
