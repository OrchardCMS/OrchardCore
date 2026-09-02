namespace OrchardCore.Users.Events;

public abstract class LogoutFormEventBase : ILogoutFormEvent
{
    public virtual Task LoggingOutAsync(IUser user, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public virtual Task LoggedOutAsync(IUser user, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
