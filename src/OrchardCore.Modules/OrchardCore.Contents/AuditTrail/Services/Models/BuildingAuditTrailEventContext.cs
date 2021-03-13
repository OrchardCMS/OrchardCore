using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Services.Models
{
    public class BuildingAuditTrailEventContext
    {
        public ContentItem ContentItem { get; }
        public string EventName { get; }
        public bool IsCanceled { get; set; }

        public BuildingAuditTrailEventContext(ContentItem contentItem, string eventName)
        {
            ContentItem = contentItem;
            EventName = eventName;
        }
    }
}
