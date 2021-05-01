using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Providers;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.Contents.AuditTrail.Services.Models;
using OrchardCore.Modules;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.Contents.AuditTrail.Handlers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class GlobalContentHandler : ContentHandlerBase, IAuditTrailContentHandler
    {
        private readonly IYesSqlSession _session;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailContentEventHandler> _auditTrailEvents;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        private HashSet<string> _restoring = new HashSet<string>();

        public GlobalContentHandler(
            IYesSqlSession session,
            IAuditTrailManager auditTrailManager,
            IEnumerable<IAuditTrailContentEventHandler> auditTrailEvents,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GlobalContentHandler> logger)
        {
            _session = session;
            _auditTrailEvents = auditTrailEvents;
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override Task DraftSavedAsync(SaveDraftContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Saved, context.ContentItem);

        public override Task CreatedAsync(CreateContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Created, context.ContentItem);

        public override Task PublishedAsync(PublishContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Published, context.ContentItem);

        public override Task UnpublishedAsync(PublishContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Unpublished, context.ContentItem);

        public override Task RemovedAsync(RemoveContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Removed, context.ContentItem);

        public override Task ClonedAsync(CloneContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Cloned, context.ContentItem);

        public Task RestoringAsync(RestoreContentContext context)
        {
            _restoring.Add(context.ContentItem.ContentItemId);
            return Task.CompletedTask;
        }

        public Task RestoredAsync(RestoreContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Restored, context.ContentItem);

        private async Task RecordAuditTrailEventAsync(string eventName, IContent content)
        {
            if (eventName != ContentAuditTrailEventProvider.Restored && _restoring.Contains(content.ContentItem.ContentItemId))
            {
                return;
            }

            var buildingAuditTrailEventContext = new BuildingAuditTrailEventContext(content.ContentItem, eventName);

            await _auditTrailEvents.InvokeAsync((provider, context) =>
                provider.BuildingAuditTrailEventAsync(context), buildingAuditTrailEventContext, _logger);

            if (buildingAuditTrailEventContext.IsCanceled) return;

            var versionNumber = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(contentItemIndex => contentItemIndex.ContentItemId == content.ContentItem.ContentItemId)
                .CountAsync();

            var eventData = new Dictionary<string, object>
            {
                { "ContentItem", content.ContentItem },
                { "VersionNumber", versionNumber }
            };

            await _auditTrailManager.AddAuditTrailEventAsync<ContentAuditTrailEventProvider>(new AuditTrailContext
            (
                eventName,
                _httpContextAccessor.GetCurrentUserName(),
                eventData,
                "content",
                content.ContentItem.ContentItemId)
            );
        }
    }
}
