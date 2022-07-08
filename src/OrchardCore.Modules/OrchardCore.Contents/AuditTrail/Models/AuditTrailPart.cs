using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Models
{
    public class AuditTrailPart : ContentPart
    {
        public string Comment { get; set; }
        public bool ShowComment { get; set; }
    }
}
