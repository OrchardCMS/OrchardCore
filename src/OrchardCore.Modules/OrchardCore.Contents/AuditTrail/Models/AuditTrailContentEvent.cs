using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Models
{
    public class AuditTrailContentEvent
    {
        public string Name { get; set; }
        public ContentItem ContentItem { get; set; }
        public int VersionNumber { get; set; }
    }
}
