namespace OrchardCore.Users.Events;

public abstract class PasswordRecoveryFormEvents : IPasswordRecoveryFormEvents
{
    public virtual Task PasswordRecoveredAsync(PasswordRecoveryContext context)
        => Task.CompletedTask;

    public virtual Task PasswordResetAsync(PasswordRecoveryContext context)
        => Task.CompletedTask;

    public virtual Task RecoveringPasswordAsync(Action<string, string> reportError)
        => Task.CompletedTask;

    public virtual Task ResettingPasswordAsync(Action<string, string> reportError)
        => Task.CompletedTask;
}
