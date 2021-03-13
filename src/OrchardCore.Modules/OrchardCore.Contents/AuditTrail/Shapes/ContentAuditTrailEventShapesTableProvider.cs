using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.AuditTrail.Extensions;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DisplayManagement.Descriptors;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Shapes
{
    public class ContentAuditTrailEventShapesTableProvider : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("AuditTrailEvent").OnDisplaying(async context =>
            {
                dynamic shape = context.Shape;

                var auditTrailEvent = (AuditTrailEvent)shape.AuditTrailEvent;

                if (auditTrailEvent.Category != "Content") return;

                var contentItemJToken = auditTrailEvent.Get(auditTrailEvent.EventName);
                var contentEventJToken = auditTrailEvent.Get(nameof(ContentEvent));
                var contentItem = contentItemJToken.ToObject<ContentItem>();

                if (context.Shape.Metadata.DisplayType == "Detail")
                {
                    var session = context.ServiceProvider.GetRequiredService<ISession>();

                    AuditTrailEvent previousAuditTrailEvent = null;

                    previousAuditTrailEvent = await session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                        .Where(eventIndex =>
                            eventIndex.CreatedUtc < auditTrailEvent.CreatedUtc &&
                            eventIndex.Category == "Content" &&
                            eventIndex.EventFilterData == contentItem.ContentItemId)
                        .OrderByDescending(eventIndex => eventIndex.CreatedUtc)
                        .FirstOrDefaultAsync();

                    if (previousAuditTrailEvent != null)
                    {
                        var contentDefinitionManager = context.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                        var previousContentItemJToken = previousAuditTrailEvent.Get(previousAuditTrailEvent.EventName);
                        var previousContentItem = previousContentItemJToken.ToObject<ContentItem>();
                        var currentTypeDefinition = contentDefinitionManager.LoadTypeDefinition(contentItem.ContentType);
                        var previousTypeDefinition = contentDefinitionManager.LoadTypeDefinition(previousContentItem.ContentType);

                        var contentParts = currentTypeDefinition.Parts
                            .Select(typePartDefinition => typePartDefinition.Name)
                            .Intersect(previousTypeDefinition.Parts.Select(b => b.Name));

                        JObject diff = JsonExtensions.FindDiff(contentItem.Content, previousContentItem.Content);
                        var diffNodes = new List<DiffNode>();

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

                        shape.DiffNodes = diffNodes;
                    }
                }

                shape.ContentItem = contentItem;
                shape.VersionNumber = contentEventJToken.Value<int>(nameof(ContentEvent.VersionNumber));
            });

            builder.Describe("AuditTrailEventActions").OnDisplaying(context =>
            {
                dynamic shape = context.Shape;

                var auditTrailEvent = (AuditTrailEvent)shape.AuditTrailEvent;

                if (auditTrailEvent.Category != "Content") return;

                var contentItem = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();
                var contentEventJToken = auditTrailEvent.Get(nameof(ContentEvent));

                shape.ContentItem = contentItem;
                shape.VersionNumber = contentEventJToken.Value<int>(nameof(ContentEvent.VersionNumber));
            });
        }
    }
}
