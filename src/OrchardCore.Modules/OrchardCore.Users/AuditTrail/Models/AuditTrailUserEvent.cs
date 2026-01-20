namespace OrchardCore.Users.AuditTrail.Models
{
    public class AuditTrailUserEvent
    {
        // TODO a future version should also record the User state, enabling diff against users
        public string Name { get; set; } = "User";
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
}
