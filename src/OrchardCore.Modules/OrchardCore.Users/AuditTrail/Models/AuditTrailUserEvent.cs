namespace OrchardCore.Users.AuditTrail.Models
{
    public class AuditTrailUserEvent
    {
        public string EventName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
}
