using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Contents.AuditTrail.Services
{
    public class ContentAuditTrailEventConfiguration : IConfigureOptions<AuditTrailOptions>
    {
        public const string Content = nameof(Content);
        public const string Created = nameof(Created);
        public const string Saved = nameof(Saved);
        public const string Published = nameof(Published);
        public const string Unpublished = nameof(Unpublished);
        public const string Removed = nameof(Removed);
        public const string Cloned = nameof(Cloned);
        public const string Restored = nameof(Restored);

        public void Configure(AuditTrailOptions options)
        {
            options.For<ContentAuditTrailEventConfiguration>(Content, S => S["Content"])
                .WithEvent(Created, S => S["Created"], S => S["A content item was created."], true)
                .WithEvent(Saved, S => S["Saved"], S => S["A content item was saved."], true)
                .WithEvent(Published, S => S["Published"], S => S["A content item was published."], true)
                .WithEvent(Unpublished, S => S["Unpublished"], S => S["A content item was unpublished."], true)
                .WithEvent(Removed, S => S["Removed"], S => S["A content item was deleted."], true)
                .WithEvent(Cloned, S => S["Cloned"], S => S["A content item was cloned."], true)
                .WithEvent(Restored, S => S["Restored"], S => S["A content item was restored to a previous version."], true);
        }
    }
}
