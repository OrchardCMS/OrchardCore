using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;

namespace OrchardCore.Contents.AuditTrail.Providers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
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
            context.For<ContentAuditTrailEventProvider>("Content", T["Content"])
                .Event(Created, T["Created"], T["A content item was created."], BuildEvent, true)
                .Event(Saved, T["Saved"], T["A content item was saved."], BuildEvent, true)
                .Event(Published, T["Published"], T["A content item was published."], BuildEvent, true)
                .Event(Unpublished, T["Unpublished"], T["A content item was unpublished."], BuildEvent, true)
                .Event(Removed, T["Removed"], T["A content item was deleted."], BuildEvent, true)
                .Event(Cloned, T["Cloned"], T["A content item was cloned."], BuildEvent, true)
                .Event(Restored, T["Restored"], T["A content item was restored to a previous version."], BuildEvent, true);

        private void BuildEvent(AuditTrailEvent @event, Dictionary<string, object> eventData)
        {
            @event.Put(new AuditTrailContentEvent
            {
                EventName = @event.Name,
                ContentItem = eventData.Get<ContentItem>("ContentItem"),
                VersionNumber = eventData.Get<int>("VersionNumber"),
            });
        }
    }
}
