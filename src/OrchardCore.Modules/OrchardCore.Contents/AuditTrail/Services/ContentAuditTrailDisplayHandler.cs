using System.Collections.Generic;
using System.Linq;
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
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Extensions;
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
        private readonly IContentDefinitionManager _contentDefinitionManager;

        private readonly Dictionary<string, int> _versionsCounts = new Dictionary<string, int>();

        public ContentAuditTrailDisplayHandler(ISession session, IContentDefinitionManager contentDefinitionManager)
        {
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override async Task DisplayEventAsync(DisplayEventContext context)
        {
            var model = context.EventShape.As<AuditTrailEventViewModel>();
            if (model.AuditTrailEvent.Category == "Content")
            {
                if (context.EventShape.Metadata.DisplayType == "Detail")
                {
                    var diffNodes = await BuildDiffNodesAsync(model.AuditTrailEvent);
                    context.EventShape.Properties["DiffNodes"] = diffNodes;
                }

                var contentItem = model.AuditTrailEvent.As<AuditTrailContentEvent>().ContentItem;
                if (!_versionsCounts.TryGetValue(contentItem.ContentItemId, out var versionsCount))
                {
                    versionsCount = await _session.Query<ContentItem, ContentItemIndex>()
                        .Where(index => index.ContentItemId == contentItem.ContentItemId)
                        .CountAsync();

                    _versionsCounts[contentItem.ContentItemId] = versionsCount;
                }

                context.EventShape.Properties["VersionsCount"] = versionsCount;
            }
        }

        private async Task<List<DiffNode>> BuildDiffNodesAsync(AuditTrailEvent auditTrailEvent)
        {
            var contentItem = auditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

            var previousAuditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(eventIndex =>
                    eventIndex.Category == "Content" &&
                    eventIndex.CreatedUtc < auditTrailEvent.CreatedUtc &&
                    eventIndex.EventFilterData == contentItem.ContentItemId)
                .OrderByDescending(eventIndex => eventIndex.Id)
                .FirstOrDefaultAsync();

            if (previousAuditTrailEvent == null)
            {
                return null;
            }

            if (!contentItem.CreatedUtc.HasValue)
            {
                contentItem = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemVersionId == contentItem.ContentItemVersionId)
                    .FirstOrDefaultAsync();

                if (contentItem == null)
                {
                    return null;
                }
            }

            var previousContentItem = previousAuditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

            if (!previousContentItem.CreatedUtc.HasValue)
            {
                previousContentItem = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemVersionId == previousContentItem.ContentItemVersionId)
                    .FirstOrDefaultAsync();

                if (previousContentItem == null)
                {
                    return null;
                }
            }

            if (previousContentItem.ContentType == contentItem.ContentType)
            {
                var contentTypeDefinition = _contentDefinitionManager.LoadTypeDefinition(contentItem.ContentType);

                JObject diff = JsonExtensions.FindDiff(contentItem.Content, previousContentItem.Content);
                var diffNodes = new List<DiffNode>();

                var contentParts = contentTypeDefinition.Parts.Select(typePartDefinition => typePartDefinition.Name);
                foreach (var contentPart in contentParts)
                {
                    if (diff.ContainsKey(contentPart))
                    {
                        foreach (var diffNode in JsonExtensions.GenerateDiffNodes(diff[contentPart]))
                        {
                            diffNode.Context = contentItem.ContentType + "/" + diffNode.Context;
                            diffNodes.Add(diffNode);
                        }
                    }
                }

                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.Author)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.ContentItemVersionId)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.CreatedUtc)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.DisplayText)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.Latest)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.Owner)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.ModifiedUtc)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.Published)));
                diffNodes.AddToListIfNotNull(
                    contentItem.LogPropertyChange(previousContentItem, nameof(contentItem.PublishedUtc)));

                return diffNodes;
            }

            return null;
        }
    }
}
