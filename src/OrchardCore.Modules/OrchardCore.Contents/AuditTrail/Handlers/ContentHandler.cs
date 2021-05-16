using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Providers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Handlers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class ContentHandler : ContentHandlerBase, IAuditTrailContentHandler
    {
        private readonly YesSql.ISession _session;
        private readonly ISiteService _siteService;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HashSet<string> _restoring = new HashSet<string>();

        public ContentHandler(
            YesSql.ISession session,
            ISiteService siteService,
            IAuditTrailManager auditTrailManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _siteService = siteService;
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;
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

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var settings = siteSettings.As<AuditTrailSettings>();
            if (!settings.AllowedContentTypes.Contains(content.ContentItem.ContentType))
            {
                return;
            }

            var versionNumber = await _session.QueryIndex<ContentItemIndex>()
                .Where(index => index.ContentItemId == content.ContentItem.ContentItemId)
                .CountAsync();

            var eventData = new Dictionary<string, object>
            {
                { "ContentItem", content.ContentItem },
                { "VersionNumber", versionNumber }
            };

            await _auditTrailManager.RecordEventAsync<ContentAuditTrailEventProvider>(
                new AuditTrailContext
                (
                    "Content",
                    eventName,
                    content.ContentItem.ContentItemId,
                    _httpContextAccessor.GetCurrentUserName(),
                    eventData
                ));
        }
    }
}
