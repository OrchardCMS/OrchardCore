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

        public ContentAuditTrailEventProvider(IStringLocalizer<ContentAuditTrailEventProvider> stringLocalizer) : base(stringLocalizer)
        { }


        public override void Describe(DescribeContext context)
        {
            if (context.Category != null && context.Category != "Content")
            {
                return;
            }

            context.For("Content", S["Content"])
                .Event(Created, S["Created"], S["A content item was created."], true)
                .Event(Saved, S["Saved"], S["A content item was saved."], true)
                .Event(Published, S["Published"], S["A content item was published."], true)
                .Event(Unpublished, S["Unpublished"], S["A content item was unpublished."], true)
                .Event(Removed, S["Removed"], S["A content item was deleted."], true)
                .Event(Cloned, S["Cloned"], S["A content item was cloned."], true)
                .Event(Restored, S["Restored"], S["A content item was restored to a previous version."], true);
        }
    }
}
