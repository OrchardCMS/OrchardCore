namespace OrchardCore.Users.Events;

/// <summary>
/// Context for password recovery events.
/// </summary>
public class PasswordRecoveryContext
{
    public PasswordRecoveryContext(IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
    }

    public IUser User { get; }
}
