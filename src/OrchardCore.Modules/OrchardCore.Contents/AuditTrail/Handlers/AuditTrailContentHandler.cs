using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Handlers
{
    public class AuditTrailContentHandler : ContentHandlerBase
    {
        private readonly YesSql.ISession _session;
        private readonly ISiteService _siteService;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HashSet<string> _restoring = new HashSet<string>();

        public AuditTrailContentHandler(
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

        public override Task DraftSavedAsync(SaveDraftContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Saved, context.ContentItem);

        public override Task CreatedAsync(CreateContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Created, context.ContentItem);

        public override Task PublishedAsync(PublishContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Published, context.ContentItem);

        public override Task UnpublishedAsync(PublishContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Unpublished, context.ContentItem);

        public override Task RemovedAsync(RemoveContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Removed, context.ContentItem);

        public override Task ClonedAsync(CloneContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Cloned, context.ContentItem);

        public override Task RestoringAsync(RestoreContentContext context)
        {
            _restoring.Add(context.ContentItem.ContentItemId);

            return Task.CompletedTask;
        }

        public override Task RestoredAsync(RestoreContentContext context)
            => RecordAuditTrailEventAsync(ContentAuditTrailEventConfiguration.Restored, context.ContentItem);

        private async Task RecordAuditTrailEventAsync(string name, IContent content)
        {
            if (name != ContentAuditTrailEventConfiguration.Restored && _restoring.Contains(content.ContentItem.ContentItemId))
            {
                return;
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var settings = siteSettings.As<ContentAuditTrailSettings>();
            if (!settings.AllowedContentTypes.Contains(content.ContentItem.ContentType))
            {
                return;
            }

            var versionNumber = await _session
                .QueryIndex<ContentItemIndex>(index => index.ContentItemId == content.ContentItem.ContentItemId)
                .CountAsync();

            await _auditTrailManager.RecordEventAsync(
                new AuditTrailContext<AuditTrailContentEvent>
                (
                    name,
                    ContentAuditTrailEventConfiguration.Content,
                    content.ContentItem.ContentItemId,
                    _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                    _httpContextAccessor.HttpContext.User?.Identity?.Name,
                    new AuditTrailContentEvent
                    {
                        ContentItem = content.ContentItem,
                        VersionNumber = versionNumber
                    }
                ));
        }
    }
}
