using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Modules;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.AuditTrail.Handlers
{
    public class GlobalContentHandler : ContentHandlerBase
    {
        private readonly IYesSqlSession _session;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IAuditTrailContentEventHandler> _auditTrailEvents;

        public ILogger Logger { get; set; }

        public GlobalContentHandler(
            IYesSqlSession session,
            ILogger<GlobalContentHandler> logger,
            IAuditTrailManager auditTrailManager,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IAuditTrailContentEventHandler> auditTrailEvents)
        {
            _session = session;
            _auditTrailEvents = auditTrailEvents;
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;

            Logger = logger;
        }

        public override Task DraftSavedAsync(SaveDraftContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Saved, context.ContentItem);

        public override Task CreatedAsync(CreateContentContext context)
        {
            var auditTrailRestoreFeature = _httpContextAccessor.HttpContext.Features.Get<AuditTrailRestoreFeature>();

            if (auditTrailRestoreFeature != null)
            {
                return RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Restored, auditTrailRestoreFeature.ContentItem);
            }
            else if (!context.ContentItem.Published)
            {
                return RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Created, context.ContentItem);
            }

            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Published, context.ContentItem);

        public override Task UnpublishedAsync(PublishContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Unpublished, context.ContentItem);

        public override Task RemovedAsync(RemoveContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Removed, context.ContentItem);

        public override Task ClonedAsync(CloneContentContext context) =>
            RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Cloned, context.ContentItem);

        private async Task RecordAuditTrailEventAsync(string eventName, IContent content)
        {
            var buildingAuditTrailEventContext = new BuildingAuditTrailEventContext(content.ContentItem, eventName);

            await _auditTrailEvents.InvokeAsync((provider, context) =>
                provider.BuildingAuditTrailEventAsync(context), buildingAuditTrailEventContext, Logger);

            if (buildingAuditTrailEventContext.IsCanceled) return;

            var versionNumber = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(contentItemIndex => contentItemIndex.ContentItemId == content.ContentItem.ContentItemId)
                .CountAsync();

            var eventData = new Dictionary<string, object>
            {
                { "ContentItem", content.ContentItem },
                { "VersionNumber", versionNumber }
            };

            await _auditTrailManager.AddAuditTrailEventAsync<ContentAuditTrailEventProvider>(
                new AuditTrailContext(eventName, _httpContextAccessor.GetCurrentUserName(), eventData, "content", content.ContentItem.ContentItemId));
        }
    }
}
