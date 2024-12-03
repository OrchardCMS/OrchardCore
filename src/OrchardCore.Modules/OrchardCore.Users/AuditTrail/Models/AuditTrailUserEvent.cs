using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.Models;

// TODO a future version should also record the User state, enabling diff against users
public class AuditTrailUserEvent
{
    public string Name { get; set; } = "User";

    public string UserName { get; set; }

    public string UserId { get; set; }

    /// <summary>
    /// The date and time when the user account was created.
    /// </summary>
    public DateTime? CreatedUtc { get; set; }

    /// <summary>
    /// The ID of the user who created this account.
    /// </summary>
    public string CreatedById { get; set; }

    /// <summary>
    ///The date and time when the user was last modified.
    /// </summary>
    public DateTime? ModifiedUtc { get; set; }

    /// <summary>
    /// The ID of the user who last modified this record.
    /// </summary>
    public string ModifiedById { get; set; }

    public AuditTrailUserEvent()
    {

    }

    public AuditTrailUserEvent(IUser user)
    {
        UserName = user.UserName;

        if (user is User u)
        {
            UserId = u.UserId;
            CreatedUtc = u.CreatedUtc;
            CreatedById = u.CreatedById;
            ModifiedUtc = u.ModifiedUtc;
            ModifiedById = u.ModifiedById;
        }
    }
}
