using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Extensions;
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
        private readonly IHttpContextAccessor _hca;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IEnumerable<IAuditTrailContentEventHandler> _auditTrailEvents;

        public ILogger Logger { get; set; }


        public GlobalContentHandler(
            IYesSqlSession session,
            IHttpContextAccessor hca,
            ILogger<GlobalContentHandler> logger,
            IAuditTrailManager auditTrailManager,
            IEnumerable<IAuditTrailContentEventHandler> auditTrailEvents)
        {
            _hca = hca;
            _session = session;
            _auditTrailManager = auditTrailManager;
            _auditTrailEvents = auditTrailEvents;

            Logger = logger;
        }


        public override Task UpdatedAsync(UpdateContentContext context) =>
            // The Document.Id of the child content items will always be 0. We don't need to log the children.
            // In this case we can prevent to log the items inside the FlowPart, BagPart, MenuItemLink, etc.
            context.ContentItem.Id != 0 ?
                RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Saved, context.ContentItem) :
                Task.CompletedTask;

        public override Task CreatedAsync(CreateContentContext context)
        {
            if (_hca.HttpContext.Items.ContainsKey("OrchardCore.AuditTrail.Restored"))
            {
                return RecordAuditTrailEventAsync(ContentAuditTrailEventProvider.Restored, context.ContentItem);
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
            // -1 is a unique contentItem.Id that only Preview is using such that another stored document can't have
            // the same one in the IContentManagerSession index.
            // ToDo: use the ContentPreviewFeature from this PR. https://github.com/OrchardCMS/OrchardCore/pull/6626
            if (content.ContentItem.Id == -1) return;

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
                new AuditTrailContext(eventName, _hca.GetCurrentUserName(), eventData, "content", content.ContentItem.ContentItemId));
        }
    }
}
