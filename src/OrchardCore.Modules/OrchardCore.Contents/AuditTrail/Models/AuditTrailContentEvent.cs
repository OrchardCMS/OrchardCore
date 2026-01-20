using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Models
{
    public class AuditTrailContentEvent
    {
        public string Name { get; set; } = "Content";
        public ContentItem ContentItem { get; set; }
        public int VersionNumber { get; set; }
        public string Comment { get; set; }
    }
}
