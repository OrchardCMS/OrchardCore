using System;

namespace OrchardCore.Contents.AuditTrail.Settings
{
    public class ContentAuditTrailSettings
    {
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    }
}
