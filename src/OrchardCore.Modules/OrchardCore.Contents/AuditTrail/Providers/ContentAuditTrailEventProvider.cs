using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
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

        private IContentDefinitionManager _contentDefinitionManager;

        public ContentAuditTrailEventProvider(
            IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<ContentAuditTrailEventProvider> stringLocalizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
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

            // The whole content item is embedded only if not versionable to still keep track of changes.
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition?.GetSettings<ContentTypeSettings>().Versionable ?? true)
            {
                contentItem = new ContentItem()
                {
                    ContentItemId = contentItem.ContentItemId,
                    ContentItemVersionId = contentItem.ContentItemVersionId,
                    ContentType = contentItem.ContentType,
                };
            }

            auditTrailEvent.Put(new AuditTrailContentEvent
            {
                EventName = auditTrailEvent.EventName,
                ContentItem = contentItem,
                VersionNumber = eventData.Get<int>("VersionNumber"),
            });
        }
    }
}
