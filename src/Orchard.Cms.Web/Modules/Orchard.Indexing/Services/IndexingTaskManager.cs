using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.Indexing.Models;
using Orchard.Services;
using YesSql.Core.Services;
using YesSql.Core.Sql;

namespace Orchard.Indexing.Services
{
    public class IndexingTaskManager
    {
        private readonly IClock _clock;
        private readonly ISession _session;
        private readonly string _tablePrefix;

        public IndexingTaskManager(
            IStore store,
            ISession session,
            IContentManager contentManager,
            IClock clock,
            ILogger<IndexingTaskManager> logger)
        {
            _session = session;
            _clock = clock;
            Logger = logger;

            _tablePrefix = store.Configuration.TablePrefix;
        }

        public ILogger Logger { get; set; }

        private async Task CreateTaskAsync(ContentItem contentItem, IndexingTaskTypes type)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException("contentItem");
            }

            if (contentItem.Id == 0)
            {
                // Ignore that case, when Update is called on a content item which has not be "created" yet

                return;
            }

            var contentItemId = contentItem.Id;
            var builder = new SqlBuilder();

            var transaction = _session.Demand();
            var connection = transaction.Connection;
            var table = $"{_tablePrefix }{ nameof(IndexingTask)}";

            // TODO: Maybe ignore deleting previous items, and do it when indexing is actually executed
            // or this would issue these commands for every change on content items.

            // Delete any previous indexing task on this content item
            var deleteCmd = $"delete from [{table}] where [ContentItemId] = @Id;";
            await connection.ExecuteAsync(deleteCmd, new { contentItem.Id }, transaction);
            
            var taskRecord = new IndexingTask
            {
                CreatedUtc = _clock.UtcNow,
                ContentItemId = contentItem.Id,
                Type = type
            };

            var insertCmd = $"insert into [{table}] ([CreatedUtc], [ContentItemId], [Type]) values (@CreatedUtc, @ContentItemId, @Type);";
            await connection.ExecuteAsync(insertCmd, taskRecord, transaction);
        }

        public async Task CreateUpdateIndexTaskAsync(ContentItem contentItem)
        {
            await CreateTaskAsync(contentItem, IndexingTaskTypes.Update);

            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Indexing task created for [{0}:{1}]", contentItem.ContentType, contentItem.Id);
            }
        }

        public async Task CreateDeleteIndexTaskAsync(ContentItem contentItem)
        {
            await CreateTaskAsync(contentItem, IndexingTaskTypes.Delete);

            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Deleting index task created for [{0}:{1}]", contentItem.ContentType, contentItem.Id);
            }
        }
    }
}
