namespace OrchardCore.Users.AuditTrail.Models;

// TODO a future version should also record the User state, enabling diff against users
public class AuditTrailUserEvent
{
    public string Name { get; set; } = "User";

    public string UserName { get; set; }

    public string UserId { get; set; }
}
