namespace OrchardCore.Users.Events;

public class UserRegisteringContext
{
    public bool Cancel { get; set; }

    public IUser User { get; }

    public UserRegisteringContext(IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
    }
}
