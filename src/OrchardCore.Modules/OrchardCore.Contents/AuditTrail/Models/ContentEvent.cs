namespace OrchardCore.Contents.AuditTrail.Models
{
    public class ContentEvent
    {
        public string ContentItemId { get; set; }
        public string ContentType { get; set; }
        public int VersionNumber { get; set; }
        public string EventName { get; set; }
        public bool Published { get; set; }
    }
}
