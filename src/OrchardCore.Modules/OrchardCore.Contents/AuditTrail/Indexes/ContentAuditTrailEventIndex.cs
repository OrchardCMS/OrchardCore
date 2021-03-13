using YesSql.Indexes;

namespace OrchardCore.Contents.AuditTrail.Indexes
{
    /// <summary>
    /// Used to index the Content Audit Trail Events.
    /// </summary>
    public class ContentAuditTrailEventIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string ContentType { get; set; }
        public int VersionNumber { get; set; }
        public string EventName { get; set; }
        public bool Published { get; set; }
    }
}
