namespace OrchardCore.Users.Handlers;

/// <summary>
/// Represents a context for confirming a user.
/// </summary>
public class UserConfirmContext : UserContext
{
    /// <inheritdocs />
    public UserConfirmContext(IUser user)
        : base(user)
    {
    }

    public UserConfirmationType ConfirmationType { get; set; }
}
