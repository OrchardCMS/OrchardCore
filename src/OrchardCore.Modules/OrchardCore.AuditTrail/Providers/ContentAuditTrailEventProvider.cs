using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Providers
{
    public class ContentAuditTrailEventProvider : AuditTrailEventProviderBase
    {
        public const string Created = nameof(Created);
        public const string Saved = nameof(Saved);
        public const string Published = nameof(Published);
        public const string Unpublished = nameof(Unpublished);
        public const string Removed = nameof(Removed);
        public const string Cloned = nameof(Cloned);
        public const string Restored = nameof(Restored);


        public ContentAuditTrailEventProvider(IStringLocalizer<ContentAuditTrailEventProvider> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public override void Describe(DescribeContext context) =>
            context.For<ContentAuditTrailEventProvider>("Content", T["Content Item"])
                .Event(Created, T["Created"], T["A content item was created."], BuildAuditTrailEvent, true)
                .Event(Saved, T["Saved"], T["A content item was saved."], BuildAuditTrailEvent, true)
                .Event(Published, T["Published"], T["A content item was published."], BuildAuditTrailEvent, true)
                .Event(Unpublished, T["Unpublished"], T["A content item was unpublished."], BuildAuditTrailEvent, true)
                .Event(Removed, T["Removed"], T["A content item was deleted."], BuildAuditTrailEvent, true)
                .Event(Cloned, T["Cloned"], T["A content item was cloned."], BuildAuditTrailEvent, true)
                .Event(Restored, T["Restored"], T["A content item was restored to a previous version."], BuildAuditTrailEvent, true);


        private void BuildAuditTrailEvent(AuditTrailEvent auditTrailEvent, Dictionary<string, object> eventData)
        {
            var contentItem = eventData.Get<ContentItem>("ContentItem");
            auditTrailEvent.Put(auditTrailEvent.EventName, contentItem);
            auditTrailEvent.Put(new ContentEvent
            {
                ContentItemId = contentItem.ContentItemId,
                ContentType = contentItem.ContentType,
                VersionNumber = eventData.Get<int>("VersionNumber"),
                EventName = auditTrailEvent.EventName,
                Published = contentItem.IsPublished()
            });
        }
    }
}
