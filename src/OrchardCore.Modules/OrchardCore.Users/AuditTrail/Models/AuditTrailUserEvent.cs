using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.Models;

/// <summary>
/// Event data for an Audit Trail event related to <see cref="Users.Models.User"/>s.
/// </summary>
public class AuditTrailUserEvent
{
    /// <summary>
    /// Gets the event name.
    /// </summary>
    public string Name => "User";
    
    /// <summary>
    /// Gets or sets a snapshot of the <see cref="Users.Models.User"/> object, if the event modified it somehow.
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// Gets or sets the related user's <see cref="Users.Models.User.UserName"/>.
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Gets or sets the related user's <see cref="Users.Models.User.UserId"/>.
    /// </summary>
    public string UserId { get; set; }
}
