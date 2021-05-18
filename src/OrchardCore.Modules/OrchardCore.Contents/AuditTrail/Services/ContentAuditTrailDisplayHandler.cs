using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Services
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class ContentAuditTrailDisplayHandler : AuditTrailDisplayHandlerBase
    {
        private readonly ISession _session;

        private readonly Dictionary<string, string> _latestVersionId = new Dictionary<string, string>();

        public ContentAuditTrailDisplayHandler(ISession session)
        {
            _session = session;
        }

        public override async Task DisplayEventAsync(DisplayEventContext context)
        {
            var model = context.Shape.As<AuditTrailEventViewModel>();
            if (model.Event.Category == "Content")
            {
                if (context.Shape.Metadata.DisplayType == "Detail")
                {
                    var diffNodes = await BuildDiffNodesAsync(model.Event);
                    context.Shape.Properties["DiffNodes"] = diffNodes;
                }

                var contentItem = model.Event.As<AuditTrailContentEvent>().ContentItem;
                if (!_latestVersionId.TryGetValue(contentItem.ContentItemId, out var latestVersionId))
                {
                    latestVersionId = (await _session.QueryIndex<ContentItemIndex>()
                        .Where(index => index.ContentItemId == contentItem.ContentItemId && index.Latest)
                        .FirstOrDefaultAsync())
                        ?.ContentItemVersionId;

                    _latestVersionId[contentItem.ContentItemId] = latestVersionId;
                }

                context.Shape.Properties["LatestVersionId"] = latestVersionId;
            }
        }

        private async Task<DiffNode[]> BuildDiffNodesAsync(AuditTrailEvent auditTrailEvent)
        {
            var contentItem = auditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

            var previousAuditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index =>
                    index.Category == "Content" &&
                    index.CreatedUtc <= auditTrailEvent.CreatedUtc &&
                    index.EventId != auditTrailEvent.EventId &&
                    index.CorrelationId == contentItem.ContentItemId)
                .OrderByDescending(index => index.Id)
                .FirstOrDefaultAsync();

            if (previousAuditTrailEvent == null)
            {
                return null;
            }

            var previousContentItem = previousAuditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

            var current = JObject.FromObject(contentItem);
            var previous = JObject.FromObject(previousContentItem);
            previous.Remove(nameof(AuditTrailPart));
            current.Remove(nameof(AuditTrailPart));

            if (current.FindDiff(previous, out var diff))
            {
                return diff.GenerateDiffNodes(contentItem.ContentType);
            }

            return null;
        }
    }
}
