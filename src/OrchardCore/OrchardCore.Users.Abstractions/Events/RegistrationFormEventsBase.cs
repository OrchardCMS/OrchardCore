namespace OrchardCore.Users.Events;

public abstract class RegistrationFormEventsBase : IRegistrationFormEvents
{
    public virtual Task RegisteredAsync(IUser user)
        => Task.CompletedTask;

    public virtual Task RegisteringAsync(UserRegisteringContext context)
        => Task.CompletedTask;

    public virtual Task RegistrationValidationAsync(Action<string, string> reportError)
        => Task.CompletedTask;
}
