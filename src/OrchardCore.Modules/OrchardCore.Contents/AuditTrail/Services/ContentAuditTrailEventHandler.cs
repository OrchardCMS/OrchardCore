using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;
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
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Services
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class ContentAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly ISession _session;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentAuditTrailEventHandler(ISession session, IContentDefinitionManager contentDefinitionManager)
        {
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context.EventFilterKey != "Content")
            {
                return Task.CompletedTask;
            }

            var content = context.EventData.Get<ContentItem>("ContentItem");
            if (content == null)
            {
                return Task.CompletedTask;
            }

            var auditTrailPart = content.ContentItem.As<AuditTrailPart>();
            if (auditTrailPart == null)
            {
                return Task.CompletedTask;
            }

            context.Comment = auditTrailPart.Comment;

            return Task.CompletedTask;
        }

        public override async Task BuildViewModelAsync(AuditTrailEventViewModel model)
        {
            // TODO: Seems to be done for each event !!!!
            if (model.AuditTrailEvent.Category == "Content")
            {
                var shape = (IShape)model;
                if (shape.Metadata.DisplayType == "Detail")
                {
                    await BuildDiffNodesAsync(model);
                }

                var contentItem = model.AuditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

                var availableVersionsCount = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(index => index.ContentItemId == contentItem.ContentItemId)
                    .CountAsync();

                shape.Properties["AvailableVersionsCount"] = availableVersionsCount;
            }
        }

        private async Task BuildDiffNodesAsync(AuditTrailEventViewModel model)
        {
            var auditTrailEvent = model.AuditTrailEvent;
            if (auditTrailEvent.Category != "Content")
            {
                return;
            }

            var shape = (IShape)model;
            if (shape.Metadata.DisplayType == "Detail")
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
                    return;
                }

                if (!contentItem.CreatedUtc.HasValue)
                {
                    contentItem = await _session.Query<ContentItem, ContentItemIndex>()
                        .Where(x => x.ContentItemVersionId == contentItem.ContentItemVersionId)
                        .FirstOrDefaultAsync();

                    if (contentItem == null)
                    {
                        return;
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
                        return;
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

                    shape.Properties["DiffNodes"] = diffNodes;
                }
            }
        }
    }
}
