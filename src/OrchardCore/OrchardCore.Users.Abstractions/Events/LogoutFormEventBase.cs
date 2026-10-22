namespace OrchardCore.Users.Events;

public abstract class LogoutFormEventBase : ILogoutFormEvent
{
    /// <inheritdoc/>
    public virtual Task LoggingOutAsync(IUser user, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task LoggedOutAsync(IUser user, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
