namespace OrchardCore.Users.Events;

public abstract class LogoutFormEventBase : ILogoutFormEvent
{
    public virtual Task LoggingOutAsync(IUser user)
        => Task.CompletedTask;

    public virtual Task LoggedOutAsync(IUser user)
        => Task.CompletedTask;
}
