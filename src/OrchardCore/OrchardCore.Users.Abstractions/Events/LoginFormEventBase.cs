using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Users.Events;

public abstract class LoginFormEventBase : ILoginFormEvent
{
    public virtual Task LoggingInAsync(string userName, Action<string, string> reportError)
        => Task.CompletedTask;

    public virtual Task LoggingInFailedAsync(string userName)
        => Task.CompletedTask;

    public virtual Task LoggingInFailedAsync(IUser user)
        => Task.CompletedTask;

    public virtual Task IsLockedOutAsync(IUser user)
        => Task.CompletedTask;

    public virtual Task LoggedInAsync(IUser user)
        => Task.CompletedTask;

    public virtual Task<IActionResult> LoggingInAsync()
        => Task.FromResult<IActionResult>(null);

    public virtual Task<IActionResult> ValidatingLoginAsync(IUser user)
        => Task.FromResult<IActionResult>(null);
}
