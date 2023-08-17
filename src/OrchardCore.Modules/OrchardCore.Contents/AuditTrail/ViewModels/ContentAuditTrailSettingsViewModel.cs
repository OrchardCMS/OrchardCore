using System;

namespace OrchardCore.Contents.AuditTrail.ViewModels
{
    public class ContentAuditTrailSettingsViewModel
    {
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    }
}
